
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using static NewsScroll.Api.Api;
using static NewsScroll.Lists.Lists;

namespace NewsScroll
{
    class ApiUpdate
    {
        public static async Task<int> PageApiUpdate()
        {
            return await Task.Run(async () =>
            {
                try
                {
                    //Reset load variables
                    AppVariables.CurrentTotalItemsCount = 0;
                    AppVariables.CurrentFeedsLoaded = 0;
                    AppVariables.CurrentItemsLoaded = 0;
                    AppVariables.BusyApplication = true;
                    Debug.WriteLine("Started updating api (News: " + OnlineUpdateNews + " Starred: " + OnlineUpdateStarred + " Feeds: " + OnlineUpdateFeeds + ")");

                    //Check if offline mode is enabled
                    if (!ApiMessageError.Contains("Off"))
                    {
                        //Check update success
                        bool UpdateStatus = true;

                        //Check the login status
                        if ((OnlineUpdateNews || OnlineUpdateStarred || OnlineUpdateFeeds) && !AppVariables.LoadSearch && !CheckLogin())
                        {
                            UpdateStatus = await Login(false, false);
                            if (UpdateStatus)
                            {
                                ApiMessageError = string.Empty;
                            }
                            else
                            {
                                List<string> messageAnswers = new List<string>();
                                messageAnswers.Add("Retry to login");
                                messageAnswers.Add("Go to account settings");
                                messageAnswers.Add("Switch to offline mode");

                                string messageResult = await MessagePopup.Popup("Failed to login", "Would you like to retry to login to The Old Reader or do you want to switch to offline mode?\n\nMake sure that you have an internet connection and that your correct account settings are set before retrying.", messageAnswers);
                                if (messageResult == "Retry to login")
                                {
                                    return await PageApiUpdate();
                                }
                                else if (messageResult == "Go to account settings")
                                {
                                    AppVariables.BusyApplication = false;
                                    return 2;
                                }
                                else
                                {
                                    ApiMessageError = "(Off) ";
                                }
                            }
                        }

                        //Download feeds from Api
                        if (UpdateStatus && AppVariables.LoadFeeds && OnlineUpdateFeeds && !ApiMessageError.Contains("Off"))
                        {
                            UpdateStatus = await Feeds(false, false);
                            if (UpdateStatus)
                            {
                                ApiMessageError = string.Empty;
                                OnlineUpdateFeeds = false;
                            }
                            else
                            {
                                List<string> messageAnswers = new List<string>();
                                messageAnswers.Add("Retry downloading feeds");
                                messageAnswers.Add("Go to account settings");
                                messageAnswers.Add("Switch to offline mode");

                                string messageResult = await MessagePopup.Popup("Failed to load the feeds", "Would you like to retry loading the feeds or do you want to switch to offline mode?\n\nMake sure that you have an internet connection and that your correct account settings are set before retrying.", messageAnswers);
                                if (messageResult == "Retry downloading feeds")
                                {
                                    return await PageApiUpdate();
                                }
                                else if (messageResult == "Go to account settings")
                                {
                                    AppVariables.BusyApplication = false;
                                    return 2;
                                }
                                else
                                {
                                    ApiMessageError = "(Off) ";
                                    OnlineUpdateFeeds = true;
                                }
                            }
                        }

                        //Download news items from Api
                        if (UpdateStatus && AppVariables.LoadNews && OnlineUpdateNews)
                        {
                            //Remove older news items
                            await ItemsRemoveOld(false, false);

                            //Sync offline changed items
                            await SyncOfflineChanges(false, false);

                            //Download news items from Api
                            if (!ApiMessageError.Contains("Off"))
                            {
                                if (UpdateStatus) { UpdateStatus = await AllNewsItems(true, false, false, false); }
                                if (!UpdateStatus)
                                {
                                    List<string> messageAnswers = new List<string>();
                                    messageAnswers.Add("Retry downloading items");
                                    messageAnswers.Add("Switch to offline mode");

                                    string messageResult = await MessagePopup.Popup("Failed to load the items", "Would you like to retry loading the items or do you want to switch to offline mode?\n\nMake sure that you have an internet connection and that your correct account settings are set before retrying.", messageAnswers);
                                    if (messageResult == "Retry downloading items")
                                    {
                                        return await PageApiUpdate();
                                    }
                                    else
                                    {
                                        ApiMessageError = "(Off) ";
                                        OnlineUpdateNews = true;
                                    }
                                }
                            }

                            //Download read status from Api
                            if (!ApiMessageError.Contains("Off"))
                            {
                                if (UpdateStatus) { UpdateStatus = await ItemsRead(List_NewsItems, false, false); }
                                if (!UpdateStatus)
                                {
                                    List<string> messageAnswers = new List<string>();
                                    messageAnswers.Add("Retry downloading read items");
                                    messageAnswers.Add("Switch to offline mode");

                                    string messageResult = await MessagePopup.Popup("Failed to load read items", "Would you like to retry loading read items or do you want to switch to offline mode?\n\nMake sure that you have an internet connection and that your correct account settings are set before retrying.", messageAnswers);
                                    if (messageResult == "Retry downloading read items")
                                    {
                                        return await PageApiUpdate();
                                    }
                                    else
                                    {
                                        ApiMessageError = "(Off) ";
                                        OnlineUpdateNews = true;
                                    }
                                }
                            }

                            //Check if news items updated
                            if (UpdateStatus && !ApiMessageError.Contains("Off"))
                            {
                                ApiMessageError = string.Empty;
                                OnlineUpdateNews = false;
                            }
                        }

                        //Download starred items from Api
                        if (UpdateStatus && AppVariables.LoadStarred && OnlineUpdateStarred && !ApiMessageError.Contains("Off"))
                        {
                            UpdateStatus = await ItemsStarred(true, false, false);
                            if (UpdateStatus)
                            {
                                ApiMessageError = string.Empty;
                                OnlineUpdateStarred = false;
                            }
                            else
                            {
                                List<string> messageAnswers = new List<string>();
                                messageAnswers.Add("Retry downloading starred items");
                                messageAnswers.Add("Switch to offline mode");

                                string messageResult = await MessagePopup.Popup("Failed to load starred items", "Would you like to retry loading starred items or do you want to switch to offline mode?\n\nMake sure that you have an internet connection and that your correct account settings are set before retrying.", messageAnswers);
                                if (messageResult == "Retry downloading starred items")
                                {
                                    return await PageApiUpdate();
                                }
                                else
                                {
                                    ApiMessageError = "(Off) ";
                                    OnlineUpdateStarred = true;
                                }
                            }
                        }
                    }

                    Debug.WriteLine("Finished the update task.");
                    AppVariables.BusyApplication = false;
                    return 0;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to start update task: " + ex.Message);
                    AppVariables.BusyApplication = false;
                    return 1;
                }
            });
        }

        //Waiting for the database to open up
        public static async Task WaitForBusyDatabase()
        {
            try
            {
                //while (SQLConnectionLock.IsInTransaction)
                //{
                //    Debug.WriteLine("Database is currently busy, waiting...");
                //    await Task.Delay(10);
                //}
                await Task.Delay(0);
            }
            catch { }
        }

        //Waiting for the application to open up
        public static async Task WaitForBusyApplication()
        {
            try
            {
                while (AppVariables.BusyApplication)
                {
                    Debug.WriteLine("Application is currently busy, waiting...");
                    await Task.Delay(100);
                }
            }
            catch { }
        }
    }
}