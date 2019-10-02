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

        string GetWorkstationId()
        {
            if (Context.Items.TryGetValue("WorkstationId", out object workstationId))
                return (string)workstationId;
            else
            {
                _logger.LogCritical("AppHub does not contain WorkstationId!");
                throw new Exception("AppHub does not contain WorkstationId!");
            }
        }

        #region Device

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
            return base.OnDisconnectedAsync(exception);
        }

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
        public async Task<HideezErrorInfo> SaveClientEvents(WorkstationEventDto[] events)
        {
            try
            {
                if (events == null)
                    throw new ArgumentNullException(nameof(events));

                _logger.LogDebug($"[{events[0].WorkstationId}] Sent events: {string.Join("; ", events.Select(s => s.EventId))}");
                // todo: ignore not approved workstation

                // Events that duplicate ID of other events are ignored
                events = events.GroupBy(e => e.Id).Select(s => s.First()).ToArray();

                // Filter out from incomming events all those who share ID with events saved in database 
                var filtered = events.Where(e => !_workstationEventService.Query().Any(we => we.Id == e.Id)).ToList(); //TODO move to Async

                // Convert from SDK WorkstationEvent to HES WorkstationEvent
                List<WorkstationEvent> converted = new List<WorkstationEvent>();
                foreach (var dto in filtered)
                {
                    var convertedEvent =
                        new WorkstationEvent()
                        {
                            Id = dto.Id,
                            Date = dto.Date,
                            EventId = dto.EventId,
                            SeverityId = dto.SeverityId,
                            Note = dto.Note,
                            WorkstationId = dto.WorkstationId,
                            UserSession = dto.UserSession,
                            DeviceId = dto.DeviceId,
                        };

                    if (!string.IsNullOrWhiteSpace(dto.AccountName) && !string.IsNullOrWhiteSpace(dto.AccountLogin))
                    {
                        convertedEvent.DeviceAccount = await _deviceAccountService
                            .Query()
                            .Where(d => d.Name == dto.AccountName
                                     && d.Login == dto.AccountLogin
                                     && d.DeviceId == dto.DeviceId)
                            .AsNoTracking()
                            .FirstOrDefaultAsync();
                    }
                    converted.Add(convertedEvent);
                }


                var addedEvents = await _workstationEventService.AddEventsRangeAsync(converted);

                var authEventsOnly = converted.Where(e => e.EventId == WorkstationEventType.ComputerLock
                    || e.EventId == WorkstationEventType.ComputerLogoff
                    || e.EventId == WorkstationEventType.ComputerLogon
                    || e.EventId == WorkstationEventType.ComputerUnlock).ToArray();

                if (authEventsOnly.Length > 0)
                    await _workstationSessionService.UpdateWorkstationSessionsAsync(authEventsOnly);

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