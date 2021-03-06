﻿using AutoPrintr.Core.IServices;
using AutoPrintr.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AutoPrintr.Helpers
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false, IncludeExceptionDetailInFaults = true)]
    internal class WindowsServiceClient : ReliableService<IWindowsService>, IWindowsServiceCallback, IWindowsServiceClient
    {
        #region Properties
        private readonly Guid _id = Guid.NewGuid();
        private readonly Dispatcher _dispatcher;
        private readonly ILoggerService _loggerService;
        private CancellationTokenSource _cts;
        private Task _task;
        private Action _connectionFailed;

        public Action<Job> JobChangedAction { get; set; }
        #endregion

        #region Constructors
        public WindowsServiceClient(ILoggerService loggerService)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _loggerService = loggerService;

            InitializeFactory(new DuplexChannelFactory<IWindowsService>(
                new InstanceContext(this), "WindowsServiceEndpoint"));
        }
        #endregion

        #region Methods
        public async Task<bool> ConnectAsync(Action connectionFailed)
        {
            await DisconnectAsync();
            _connectionFailed = connectionFailed;

            bool result;
            try
            {
                Connect(service => service.Connect(_id));
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }

            _cts = new CancellationTokenSource();
            _task = Task.Run(
                async () => await PingByTimeout(service => service.Ping(), service => service.Connect(_id), _cts.Token), 
                _cts.Token);

            return result;
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (_cts != null)
                {
                    _cts.Cancel();
                    _cts = null;

                    if (_task != null)
                    {
                        await _task;
                        _task = null;

                        TryCall(service => service.Disconnect(_id));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
                _loggerService.WriteWarning($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
            }
        }

        public async Task<IEnumerable<Printer>> GetPrintersAsync()
        {
            try
            {
                return await TryCall(service => service.GetPrinters());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
                _loggerService.WriteWarning($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
                return null;
            }
        }

        public async Task<IEnumerable<Job>> GetJobsAsync()
        {
            try
            {
                return await TryCall(service => service.GetJobs());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
                _loggerService.WriteWarning($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
                return null;
            }
        }

        public Task<bool> PrintAsync(Job job)
            => Task.Run(() => TryCall(service =>
            {
                try
                {
                    service.Print(job);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
                    _loggerService.WriteWarning($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
                    return false;
                }
            }));

        public Task<bool> DeleteJobsAsync(Job[] jobs)
            => Task.Run(() => TryCall(service =>
            {
                try
                {
                    service.DeleteJobs(jobs);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
                    _loggerService.WriteWarning($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
                    return false;
                }
            }));

        public void JobChanged(Job job)
        {
            _dispatcher.Invoke(() =>
            {
                JobChangedAction?.Invoke(job);
            });
        }

        public void ConnectionFailed()
        {
            _dispatcher.Invoke(() =>
            {
                _connectionFailed?.Invoke();
            });
        }
        #endregion
    }
}