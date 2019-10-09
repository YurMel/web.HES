using System;
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
        private readonly IWorkstationEventService _workstationEventService;
        private readonly IWorkstationSessionService _workstationSessionService;
        private readonly IDeviceService _deviceService;
        private readonly IDeviceTaskService _deviceTaskService;
        private readonly ILogger<AppHub> _logger;

        public AppHub(IRemoteDeviceConnectionsService remoteDeviceConnectionsService,
                      IRemoteWorkstationConnectionsService remoteWorkstationConnectionsService,
                      IWorkstationEventService workstationEventService,
                      IWorkstationSessionService workstationSessionService,
                      IDeviceService deviceService,
                      IDeviceTaskService deviceTaskService,
                      ILogger<AppHub> logger)
        {
            _remoteDeviceConnectionsService = remoteDeviceConnectionsService;
            _remoteWorkstationConnectionsService = remoteWorkstationConnectionsService;
            _workstationEventService = workstationEventService;
            _workstationSessionService = workstationSessionService;
            _deviceService = deviceService;
            _deviceTaskService = deviceTaskService;
            _logger = logger;
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
            try
            {
                var httpContext = Context.GetHttpContext();
                string workstationId = httpContext.Request.Headers["WorkstationId"].ToString();

                if (string.IsNullOrWhiteSpace(workstationId))
                {
                    _logger.LogCritical($"AppHub.OnConnectedAsync - WorkstationId cannot be empty");
                }
                else
                {
                    _logger.LogCritical($"Workstation '{workstationId}' connected");
                    _remoteDeviceConnectionsService.OnAppHubConnected(workstationId, Clients.Caller);
                    Context.Items.Add("WorkstationId", workstationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "OnConnectedAsync error");
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                _remoteDeviceConnectionsService.OnAppHubDisconnected(GetWorkstationId());
                _remoteWorkstationConnectionsService.OnWorkstationDisconnected(GetWorkstationId());
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "OnDisconnectedAsync error");
            }

            return base.OnDisconnectedAsync(exception);
        }

        #region Device

        // incoming request
        public async Task OnDeviceConnected(BleDeviceDto dto)
        {
            try
            {
                // Update Battery, Firmware, State, LastSynced         
                await _deviceService.UpdateDeviceInfoAsync(dto.DeviceSerialNo, dto.Battery, dto.FirmwareVersion, dto.IsLocked);

                _remoteDeviceConnectionsService.OnDeviceConnected(dto.DeviceSerialNo, GetWorkstationId(), Clients.Caller);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "OnDeviceConnected error");
            }
        }

        // incoming request
        public Task OnDeviceDisconnected(string deviceId)
        {
            try
            {
                if (!string.IsNullOrEmpty(deviceId))
                    _remoteDeviceConnectionsService.OnDeviceDisconnected(deviceId, GetWorkstationId());
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "OnDeviceDisconnected error");
            }
            return Task.CompletedTask;
        }

        // Incomming request
        public async Task<DeviceInfoDto> GetInfoByRfid(string rfid)
        {
            try
            {
                var device = await _deviceService
                    .Query()
                    .Include(d => d.Employee)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.RFID == rfid);

                return await GetDeviceInfo(device);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "GetInfoByRfid error");
                return null;
            }
        }

        // Incomming request
        public async Task<DeviceInfoDto> GetInfoByMac(string mac)
        {
            try
            {
                var device = await _deviceService
                    .Query()
                    .Include(d => d.Employee)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.MAC == mac);

                return await GetDeviceInfo(device);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "GetInfoByMac error");
                return null;
            }
        }

        // Incomming request
        public async Task<DeviceInfoDto> GetInfoBySerialNo(string serialNo)
        {
            _logger.LogInformation($"[GetInfoBySerialNo] IN serialNo {serialNo}");

            try
            {
                var device = await _deviceService
                    .Query()
                    .Include(d => d.Employee)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == serialNo);

                _logger.LogInformation($"[GetInfoBySerialNo] deviceId from DB {device?.Id}");
                var deviceInfo = await GetDeviceInfo(device);
                _logger.LogInformation($"[GetInfoBySerialNo] deviceId from GetDeviceInfo {deviceInfo?.DeviceSerialNo}");
                return deviceInfo;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "GetInfoByMac error");
                return null;
            }
        }

        async Task<DeviceInfoDto> GetDeviceInfo(Device device)
        {
            _logger.LogInformation($"[GetDeviceInfo] {device?.Id}");
            if (device == null)
                return null;

            bool needUpdate = await _deviceTaskService
                .Query()
                .Where(t => t.DeviceId == device.Id)
                .AsNoTracking()
                .AnyAsync();
            _logger.LogInformation($"[GetDeviceInfo] needUpdate {needUpdate}");
            var info = new DeviceInfoDto()
            {
                OwnerName = device.Employee?.FullName,
                OwnerEmail = device.Employee?.Email,
                DeviceMac = device.MAC,
                DeviceSerialNo = device.Id,
                NeedUpdate = needUpdate
            };
            _logger.LogInformation($"[GetDeviceInfo] return {info?.DeviceSerialNo}");
            return info;
        }

        // Incoming request
        public async Task<HideezErrorInfo> FixDevice(string deviceId)
        {
            try
            {
                await _remoteWorkstationConnectionsService.UpdateRemoteDeviceAsync(deviceId, GetWorkstationId());
                return HideezErrorInfo.Ok;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{deviceId}] {ex.Message}");
                return new HideezErrorInfo(ex);
            }
        }

        #endregion

        #region Workstation

        // Incomming request
        public async Task<HideezErrorInfo> RegisterWorkstationInfo(WorkstationInfo workstationInfo)
        {
            try
            {
                await _remoteWorkstationConnectionsService.RegisterWorkstationInfo(Clients.Caller, workstationInfo);
                return HideezErrorInfo.Ok;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{workstationInfo?.MachineName}] {ex.Message}");
                return new HideezErrorInfo(ex);
            }
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