using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agilix.Shared;

namespace Shared
{
    public static class ThreadUtil
    {
        public static void RunWorkerThread(WaitCallback workItem, string exceptionMessage = "Unhandled thread pool exception")
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    try
                    {
                        workItem(null);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, exceptionMessage);
                    }
                } catch
                {
                }
            }, null);
        }


        public static void SafeExecute(Action run, string exceptionMessage = "Unhandled exception")
        {
            try
            {
                try
                {
                    run();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, exceptionMessage);
                }
            }
            catch
            {
            }
        }
    }
}
