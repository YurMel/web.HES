using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Entities;
using HES.Core.Interfaces;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Workstation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HES.Core.Hubs
{
    public class AppHub : Hub<IRemoteAppConnection>
    {
        private readonly IRemoteDeviceConnectionsService _remoteDeviceConnectionsService;
        private readonly IRemoteWorkstationConnectionsService _remoteWorkstationConnectionsService;
        private readonly IRemoteTaskService _remoteTaskService;
        private readonly IWorkstationEventService _workstationEventService;
        private readonly IWorkstationSessionService _workstationSessionService;
        private readonly IDeviceService _deviceService;
        private readonly IDeviceTaskService _deviceTaskService;
        private readonly IDeviceAccountService _deviceAccountService;
        private readonly ILogger<AppHub> _logger;
        private readonly IDataProtectionService _dataProtectionService;

        public AppHub(IRemoteDeviceConnectionsService remoteDeviceConnectionsService,
                      IRemoteWorkstationConnectionsService remoteWorkstationConnectionsService,
                      IRemoteTaskService remoteTaskService,
                      IWorkstationEventService workstationEventService,
                      IWorkstationSessionService workstationSessionService,
                      IDeviceService deviceService,
                      IDeviceTaskService deviceTaskService,
                      IDeviceAccountService deviceAccountService,
                      ILogger<AppHub> logger,
                      IDataProtectionService dataProtectionService)
        {
            _remoteDeviceConnectionsService = remoteDeviceConnectionsService;
            _remoteWorkstationConnectionsService = remoteWorkstationConnectionsService;
            _remoteTaskService = remoteTaskService;
            _workstationEventService = workstationEventService;
            _workstationSessionService = workstationSessionService;
            _deviceService = deviceService;
            _deviceTaskService = deviceTaskService;
            _deviceAccountService = deviceAccountService;
            _logger = logger;
            _dataProtectionService = dataProtectionService;
        }

        private string GetWorkstationId()
        {
            if (Context.Items.TryGetValue("WorkstationId", out object workstationId))
                return (string)workstationId;
            else
            {
                _logger.LogCritical("AppHub does not contain WorkstationId!");
                throw new Exception("AppHub does not contain WorkstationId!");
            }
        }
        
        public override Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            string workstationId = httpContext.Request.Headers["WorkstationId"].ToString();

            if (string.IsNullOrWhiteSpace(workstationId))
            {
                _logger.LogCritical($"AppHub.OnConnectedAsync - WorkstationId cannot be empty");
            }
            else
            {
                _remoteDeviceConnectionsService.OnAppHubConnected(workstationId, Clients.Caller);
                Context.Items.Add("WorkstationId", workstationId);
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _remoteDeviceConnectionsService.OnAppHubDisconnected(GetWorkstationId());
            _remoteWorkstationConnectionsService.OnWorkstationDisconnected(GetWorkstationId());
            return base.OnDisconnectedAsync(exception);
        }

        #region Device

        // incoming request
        public async Task OnDeviceConnected(BleDeviceDto dto)
        {
            // Update Battery, Firmware, State, LastSynced         
            await _deviceService.UpdateDeviceInfoAsync(dto.DeviceSerialNo, dto.Battery, dto.FirmwareVersion, dto.IsLocked);

            _remoteDeviceConnectionsService.OnDeviceConnected(dto.DeviceSerialNo, GetWorkstationId(), Clients.Caller);
        }

        // incoming request
        public Task OnDeviceDisconnected(string deviceId)
        {
            if (!string.IsNullOrEmpty(deviceId))
                _remoteDeviceConnectionsService.OnDeviceDisconnected(deviceId, GetWorkstationId());

            return Task.CompletedTask;
        }

        // Incomming request
        public async Task<DeviceInfoDto> GetInfoByRfid(string rfid)
        {
            var device = await _deviceService
                .Query()
                .Include(d => d.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.RFID == rfid);

            return await GetDeviceInfo(device);
        }

        // Incomming request
        public async Task<DeviceInfoDto> GetInfoByMac(string mac)
        {
            var device = await _deviceService
                .Query()
                .Include(d => d.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MAC == mac);

            return await GetDeviceInfo(device);
        }

        // Incomming request
        public async Task<DeviceInfoDto> GetInfoBySerialNo(string serialNo)
        {
            var device = await _deviceService
                .Query()
                .Include(d => d.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == serialNo);

            return await GetDeviceInfo(device);
        }

        async Task<DeviceInfoDto> GetDeviceInfo(Device device)
        {
            if (device == null)
                return null;

            bool needUpdate = await _deviceTaskService
                .Query()
                .Where(t => t.DeviceId == device.Id)
                .AsNoTracking()
                .AnyAsync();

            var info = new DeviceInfoDto()
            {
                OwnerName = device.Employee?.FullName,
                OwnerEmail = device.Employee?.Email,
                DeviceMac = device.MAC,
                DeviceSerialNo = device.Id,
                NeedUpdate = needUpdate
            };

            return info;
        }

        // Incoming request
        public async Task<HideezErrorInfo> FixDevice(string deviceId)
        {
            if (deviceId == null)
                throw new ArgumentNullException(nameof(deviceId));

            return await _remoteWorkstationConnectionsService.UpdateRemoteDeviceAsync(deviceId, GetWorkstationId());
        }

        #endregion

        #region Workstation

        // Incomming request
        public async Task<HideezErrorInfo> RegisterWorkstationInfo(WorkstationInfo workstationInfo)
        {
            if (workstationInfo == null)
                throw new ArgumentNullException(nameof(workstationInfo));

            return await _remoteWorkstationConnectionsService.RegisterWorkstationInfo(Clients.Caller, workstationInfo);
        }

        #endregion

        #region Audit

        // Incomming request
        public async Task<HideezErrorInfo> SaveClientEvents(WorkstationEventDto[] workstationEventsDto)
        {
            try
            {
                if (workstationEventsDto == null)
                    throw new ArgumentNullException(nameof(workstationEventsDto));

                _logger.LogDebug($"[{workstationEventsDto.FirstOrDefault().WorkstationId}] Sent events: {string.Join("; ", workstationEventsDto.Select(s => s.EventId))}");

                // Ignore not approved workstation
                //var workstationId = workstationEventsDto.FirstOrDefault().WorkstationId;
                //var workstaton = await _workstationService.GetByIdAsync(workstationId);
                //if (workstaton.Approved == false)
                //{
                //    return new HideezErrorInfo(HideezErrorCode.HesWorkstationNotApproved, $"{workstationId}");
                //}

                await _workstationEventService.AddEventsRangeAsync(workstationEventsDto);
                await _workstationSessionService.AddOrUpdateWorkstationSessions(workstationEventsDto);

                return HideezErrorInfo.Ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new HideezErrorInfo(ex);
            }
        }

        #endregion
    }
}