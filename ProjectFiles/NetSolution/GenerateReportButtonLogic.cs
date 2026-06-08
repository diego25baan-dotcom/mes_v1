#region Using directives
using System;
using System.Net;
using System.Net.Mail;
using FTOptix.NetLogic;
using FTOptix.UI;
using UAManagedCore;
#endregion

public class GenerateReportButtonLogic : BaseNetLogic
{
    private const string SmtpHost = "smtp.office365.com";
    private const int SmtpPort = 587;
    private const string SmtpUser = "A01664328@tec.mx";
    private const string SmtpPassword = "Mikilo1029+";
    private const string SenderEmail = "A01664328@tec.mx";

    public override void Start()
    {
        try
        {
            var button = (Button)Owner;
            button.OnMouseClick += Button_OnMouseClick;
        }
        catch (Exception ex)
        {
            Log.Error("GenerateReportButtonLogic", ex.Message);
        }
    }

    private void Button_OnMouseClick(object sender, MouseClickEvent e)
    {
        try
        {
            var button = (Button)Owner;
            var container = button.Owner as IUANode;

            if (container == null)
            {
                Log.Error("GenerateReportButtonLogic", "The button owner was not found.");
                return;
            }

            var emailTextBox = container.Get<TextBox>("email");
            var reportTypeVariable = container.GetVariable("report type");

            string recipient = emailTextBox?.Text?.Trim() ?? string.Empty;
            string reportType = reportTypeVariable?.Value?.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(recipient))
            {
                Log.Warning("GenerateReportButtonLogic", "No destination email was provided in the 'email' textbox.");
                return;
            }

            string subject = "Generated report";
            string body = $"{recipient}\n\nReport type: {reportType}";

            SendEmail(recipient, subject, body);
            Log.Info($"GenerateReportButtonLogic", $"Report email sent to {recipient}.");
        }
        catch (Exception ex)
        {
            Log.Error("GenerateReportButtonLogic", ex.Message);
        }
    }

    private static void SendEmail(string recipient, string subject, string body)
    {
        using var client = new SmtpClient(SmtpHost, SmtpPort)
        {
            Credentials = new NetworkCredential(SmtpUser, SmtpPassword),
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        using var message = new MailMessage(SenderEmail, recipient)
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        client.Send(message);
    }
}
