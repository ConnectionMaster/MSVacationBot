// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Microsoft.Extensions.Configuration;
using Luis;
using CoreBot.MSVacation.Services;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly FlightBookingRecognizer _luisRecognizer;
        protected readonly ILogger Logger;
        private string connectionName;
        private readonly MSVacationService _mSVacationService;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(FlightBookingRecognizer luisRecognizer, BookingDialog bookingDialog, ILogger<MainDialog> logger, IConfiguration configuration, MSVacationService mSVacationService)
            : base(nameof(MainDialog))
        {
            _mSVacationService = mSVacationService;
            _luisRecognizer = luisRecognizer;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            //AddDialog(bookingDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                //ActStepAsync,
                //FinalStepAsync,

                //PromptStepAsync,
                //LoginStepAsync,
                //AfterLogin,
                //AfterLoginCheck,
                LuisEchoStepAsync,
                FinalStepAsync,
            }));

            connectionName = configuration["ConnectionName"];

            AddDialog(new OAuthPrompt(
            nameof(OAuthPrompt),
            new OAuthPromptSettings
            {
                ConnectionName = connectionName,
                Text = "Please Sign In",
                Title = "Sign In",
                Timeout = 300000, // User has 5 minutes to login (1000 * 60 * 5)
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }


        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the token from the previous step. Note that we could also have gotten the
            // token directly from the prompt itself. There is an example of this in the next method.
            var tokenResponse = (TokenResponse)stepContext.Result;
            // if token exists
            if (tokenResponse != null)
            {
                //var messageText = stepContext.Options?.ToString() ?? "You are logged in";
                //var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                //await stepContext.Context.SendActivityAsync(message, cancellationToken);

                return await stepContext.NextAsync();
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful please try again."), cancellationToken);

            // return await stepContext.EndDialogAsync();
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> AfterLogin(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> AfterLoginCheck(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //var tokenResponse = (TokenResponse)stepContext.Result;
            //var messageText = tokenResponse.Token;
            //var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            //return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            var tokenResponse = (TokenResponse)stepContext.Result;

            if (tokenResponse != null)
            {
                var messageText = stepContext.Options?.ToString() ?? "You are logged in";
                var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(message, cancellationToken);

            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful please try again."), cancellationToken);
            }
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> InterruptAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (innerDc.Context.Activity.Type == ActivityTypes.Message)
            {
                var text = innerDc.Context.Activity.Text.ToLowerInvariant();

                if (text == "logout")
                {
                    // The bot adapter encapsulates the authentication processes.
                    var botAdapter = (BotFrameworkAdapter)innerDc.Context.Adapter;
                    await botAdapter.SignOutUserAsync(innerDc.Context, connectionName, null, cancellationToken);
                    await innerDc.Context.SendActivityAsync(MessageFactory.Text("You have been signed out."), cancellationToken);
                    return await innerDc.CancelAllDialogsAsync(cancellationToken);
                }
            }

            return null;
        }


        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }

            // Use the text provided in FinalStepAsync or the default if it is the first time.
            var messageText = stepContext.Options?.ToString() ?? "What can I help you with today?\nSay something like \"Request vacation on March 22 2020\"";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            if (!_luisRecognizer.IsConfigured)
            {
                // LUIS is not configured, we just run the BookingDialog path with an empty BookingDetailsInstance.
                return await stepContext.BeginDialogAsync(nameof(BookingDialog), new BookingDetails(), cancellationToken);
            }

            // Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
            var luisResult = await _luisRecognizer.RecognizeAsync<FlightBooking>(stepContext.Context, cancellationToken);
            switch (luisResult.TopIntent().intent)
            {
                case FlightBooking.Intent.BookFlight:
                    await ShowWarningForUnsupportedCities(stepContext.Context, luisResult, cancellationToken);

                    // Initialize BookingDetails with any entities we may have found in the response.
                    var bookingDetails = new BookingDetails()
                    {
                        // Get destination and origin from the composite entities arrays.
                        Destination = luisResult.ToEntities.Airport,
                        Origin = luisResult.FromEntities.Airport,
                        TravelDate = luisResult.TravelDate,
                    };

                    // Run the BookingDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
                    return await stepContext.BeginDialogAsync(nameof(BookingDialog), bookingDetails, cancellationToken);

                case FlightBooking.Intent.GetWeather:
                    // We haven't implemented the GetWeatherDialog so we just display a TODO message.
                    var getWeatherMessageText = "TODO: get weather flow here";
                    var getWeatherMessage = MessageFactory.Text(getWeatherMessageText, getWeatherMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(getWeatherMessage, cancellationToken);
                    break;

                default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way (intent was {luisResult.TopIntent().intent})";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        // Shows a warning if the requested From or To cities are recognized as entities but they are not in the Airport entity list.
        // In some cases LUIS will recognize the From and To composite entities as a valid cities but the From and To Airport values
        // will be empty if those entity values can't be mapped to a canonical item in the Airport.
        private static async Task ShowWarningForUnsupportedCities(ITurnContext context, FlightBooking luisResult, CancellationToken cancellationToken)
        {
            var unsupportedCities = new List<string>();

            var fromEntities = luisResult.FromEntities;
            if (!string.IsNullOrEmpty(fromEntities.From) && string.IsNullOrEmpty(fromEntities.Airport))
            {
                unsupportedCities.Add(fromEntities.From);
            }

            var toEntities = luisResult.ToEntities;
            if (!string.IsNullOrEmpty(toEntities.To) && string.IsNullOrEmpty(toEntities.Airport))
            {
                unsupportedCities.Add(toEntities.To);
            }

            if (unsupportedCities.Any())
            {
                var messageText = $"Sorry but the following airports are not supported: {string.Join(',', unsupportedCities)}";
                var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                await context.SendActivityAsync(message, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // If the child dialog ("BookingDialog") was cancelled, the user failed to confirm or if the intent wasn't BookFlight
            // the Result here will be null.
            //if (stepContext.Result is BookingDetails result)
            //{
            //    // Now we have all the booking details call the booking service.

            //    // If the call to the booking service was successful tell the user.

            //    var timeProperty = new TimexProperty(result.TravelDate);
            //    var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
            //    var messageText = $"I have you booked to {result.Destination} from {result.Origin} on {travelDateMsg}";
            //    var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
            //    await stepContext.Context.SendActivityAsync(message, cancellationToken);
            //}


                var messageText = "What else can I do for you?";
                messageText += $"\n\nSupported features:";
                messageText += $"\n\n1-Request vacation on a specific date";
                messageText += $"\n\n2-Request vacation on from date to date";
                messageText += $"\n\n3-Approve a person vacation";
                messageText += $"\n\n4-Show your vacation balance";
                messageText += $"\n\n5-View team vacations";
                messageText += $"\n\n6-Show your pending approvals";
                messageText += $"\n\n7-View public holidays";
                messageText += $"\n\n8-Switch your vacation date from date to another date";
                messageText += $"\n\n9-Ask about a person vacation balance";

            var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(message, cancellationToken);


            // Restart the main dialog with a different message the second time around
            var promptMessage = "Go ahead try something out! :)";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }

        private async Task<DialogTurnResult> LuisEchoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            if (!_luisRecognizer.IsConfigured)
            {
                // LUIS is not configured, we just run the BookingDialog path with an empty BookingDetailsInstance.
                return await stepContext.BeginDialogAsync(nameof(BookingDialog), new BookingDetails(), cancellationToken);
            }

            // Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
            var luisResult = await _luisRecognizer.RecognizeAsync<MSVacationBot>(stepContext.Context, cancellationToken);

            var messageText = "";

            // Extract entities

            // Person
            var persons = luisResult?.Entities?.Person;
            var personName = "";
            if (persons != null && persons.Length > 0)
            {
                personName = persons[0] ?? "";
            }

            // Vacation type
            var vacationTypes = luisResult?.Entities?.VacationTypes;
            var vacationType = "";
            if (vacationTypes != null && vacationTypes.Length > 0 && vacationTypes[0].Length > 0)
            {
                vacationType = vacationTypes[0][0] ?? "";
            }

            //Vacations dates
            var startDateArray = luisResult?.Entities?.DateStart;
            var startDate = startDateArray != null && startDateArray.Length > 0 ? startDateArray[0]?.Split('T')[0] : null;

            var endDateArray = luisResult?.Entities?.DateEnd;
            var endDate = endDateArray != null && endDateArray.Length > 0 ? endDateArray[0]?.Split('T')[0] : null;

            var dateV2Array = luisResult?.Entities?.datetime;
            var dateV2 = dateV2Array != null && dateV2Array.Length > 0 && dateV2Array[0]?.Expressions[0].Length > 0 ? dateV2Array[0]?.Expressions[0]?.Split('T')[0] : null;

            var originalDateArray = luisResult?.Entities?.originalDate;
            var originalDate = originalDateArray != null && originalDateArray.Length > 0 && originalDateArray[0]?.Expressions[0].Length > 0 ? originalDateArray[0]?.Expressions[0]?.Split('T')[0] : null;

            var newDateArray = luisResult?.Entities?.newDate;
            var newDate = newDateArray != null && newDateArray.Length > 0 && newDateArray[0]?.Expressions[0].Length > 0 ? newDateArray[0]?.Expressions[0]?.Split('T')[0] : null;

            // Vacation amount
            var vacationAmountArray = luisResult?.Entities?.VacationAmount;
            var vacationAmount = vacationAmountArray != null && vacationAmountArray.Length > 0 ? vacationAmountArray[0] : null;

            // Aprrove entities
            var approveRejectList = luisResult?.Entities?.ApproveRejectList;
            var approveVacation = true;
            if(approveRejectList != null && approveRejectList.Length > 0 && approveRejectList[0].Length > 0)
            {
                approveVacation = approveRejectList[0][0] == "Approve" ? true : false;
            }
            var status = _mSVacationService.Balance.GetStatus(Guid.NewGuid());

            switch (luisResult.TopIntent().intent)
            {
                case MSVacationBot.Intent.ApproveVacation:
                    //messageText = "Intent.ApproveVacation";
                    
                    if(approveVacation)
                    {
                        messageText = $"{personName} Vacation Approved!";
                    }else
                    {
                        messageText = $"{personName} Vacation Rejected!";
                    }
                    break;
                case MSVacationBot.Intent.BalanceStatus:
                    //messageText = "Intent.BalanceStatus";

                    //var status = BalanceService.Instance.GetStatus(Guid.NewGuid());
                    messageText = $"\nYou have {status.RemainingDays} {vacationType} days left in your balance";
                    break;
                case MSVacationBot.Intent.CollectTeamVacation:
                    {
                        //messageText = "Intent.CollectTeamVacation";
                        var requests = _mSVacationService.Requests.GetTeamVacations(Guid.NewGuid());
                        messageText = $"Your team collected vacations are:" +
                        string.Join(", ", requests.Select(request => $"{request.Employee.FirstName} from {request.StartData} to {request.EndData}"));
                        break;
                    }
                case MSVacationBot.Intent.EmployeeStatusInquiry:
                    //messageText = "Intent.EmployeeStatusInquiry";
                    
                    messageText = $"{(personName != null && personName.Length > 0 ? (personName + " has"): "You have")} {status.RemainingDays} remaining days";
                    break;
                case MSVacationBot.Intent.GetPendingApprovals:
                    {
                        var requests = _mSVacationService.Requests.GetEmployeePendingRequests(Guid.NewGuid());
                        //messageText = "Intent.GetPendingApprovals";
                        messageText = "Intent.GetPendingApprovals";
                        messageText = $"Your pending approvals are:" +
                        string.Join(", ", requests.Select(request => $"{request.Employee.FirstName} from {request.StartData} to {request.EndData}"));

                        break;
                    }
                case MSVacationBot.Intent.None:
                    //messageText = "Intent.None";
                    messageText = $"Sorry, I didn't get that. Please try asking in a different way :)";

                    break;
                case MSVacationBot.Intent.PublicHolidayAwareness:
                    //messageText = "Intent.PublicHolidayAwareness";
                    {
                        var holidays = _mSVacationService.PublicHolidays.GetPublicHolidays();
                   
                        messageText = $"Public holiday for this year are: " +
                        string.Join(", ", holidays.Select(h => $"{h.Name} on {h.Date}"));

                    }
                    break;
                case MSVacationBot.Intent.ReassignVacation:
                    //messageText = "Intent.ReassignVacation";
                    messageText = "Vacation reassigned successfully";
                    if(originalDate != null)
                    {
                        messageText += $"\n\nOriginal date : {originalDate}";
                    }
                    if(newDate != null)
                    {
                        messageText += $"\n\nNew date : {newDate}";
                    }else
                    {
                        
                        if (dateV2 != null)
                        {
                            messageText += $"\n\nNew date : {dateV2}";
                        } else
                        {
                            if (startDate != null)
                            {
                                messageText += $"\n\nNew Start date : {startDate}";
                            }

                            if (endDate != null)
                            {
                                messageText += $"\n\nEnd date : {endDate}";
                            }
                        }
                    }
                    
                    if (vacationAmount != null)
                    {
                        messageText += $"\n\nAmount : {vacationAmount}";
                    }
                    break;
                case MSVacationBot.Intent.RequestVacation:
                    //messageText = "Intent.RequestVacation";
                    messageText = "Vacation request submitted successfully";
                    
                    if(dateV2 != null)
                    {
                        messageText += $"\n\nDate : {dateV2}";
                    }else
                    {
                        if (startDate != null)
                        {
                            messageText += $"\n\nStart date : {startDate}";
                        }
                        if (endDate != null)
                        {
                            messageText += $"\n\nEnd date : {endDate}";
                        }
                    }
                    if(vacationAmount != null)
                    {
                        messageText += $"\n\nAmount : {vacationAmount}";
                    }
                    break;


                default:
                    // Catch all for unhandled intents
                    messageText = $"Sorry, I didn't get that. Please try asking in a different way (intent was {luisResult.TopIntent().intent})";
                    break;
            }
            /*//Vacations dates
            var startDate = luisResult?.Entities?.DateStart;
            var endDate = luisResult?.Entities?.DateEnd;
            var dateV2 = luisResult?.Entities?.datetime;

            var originalDate = luisResult?.Entities?.originalDate;
            var newDate = luisResult?.Entities?.newDate;

            // Vacation amount
            var vacationAmount = luisResult?.Entities?.VacationAmount;*/
            var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
            await stepContext.Context.SendActivityAsync(message, cancellationToken);

            //return await stepContext.EndDialogAsync();
            return await stepContext.NextAsync(null, cancellationToken);
        }
    }
}
