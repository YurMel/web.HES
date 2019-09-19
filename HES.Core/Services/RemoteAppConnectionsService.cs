using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using HES.Core.Interfaces;
using Hideez.SDK.Communication.Remote;
using Microsoft.Extensions.Logging;

namespace HES.Core.Services
{
    //todo - implement
    public class RemoteAppConnectionsService : IRemoteAppConnectionsService
    {
        readonly IRemoteDeviceConnectionsService _remoteDeviceConnectionsService;
        readonly IRemoteTaskService _remoteTaskService;
        readonly IEmployeeService _employeeService;
        readonly IWorkstationService _workstationService;
        readonly IWorkstationProximityDeviceService _workstationProximityDeviceService;
        readonly IWorkstationEventService _workstationEventService;
        readonly IWorkstationSessionService _workstationSessionService;
        readonly IDeviceService _deviceService;
        readonly IDeviceAccountService _deviceAccountService;
        readonly ILogger<RemoteAppConnectionsService> _logger;

        public RemoteAppConnectionsService(IRemoteDeviceConnectionsService remoteDeviceConnectionsService,
                      IRemoteTaskService remoteTaskService,
                      IEmployeeService employeeService,
                      IWorkstationService workstationService,
                      IWorkstationProximityDeviceService workstationProximityDeviceService,
                      IWorkstationEventService workstationEventService,
                      IWorkstationSessionService workstationSessionService,
                      IDeviceService deviceService,
                      IDeviceAccountService deviceAccountService,
                      ILogger<RemoteAppConnectionsService> logger)
        {
            _remoteDeviceConnectionsService = remoteDeviceConnectionsService;
            _remoteTaskService = remoteTaskService;
            _employeeService = employeeService;
            _workstationService = workstationService;
            _workstationProximityDeviceService = workstationProximityDeviceService;
            _workstationEventService = workstationEventService;
            _workstationSessionService = workstationSessionService;
            _deviceService = deviceService;
            _deviceAccountService = deviceAccountService;
            _logger = logger;
        }
    }
}
