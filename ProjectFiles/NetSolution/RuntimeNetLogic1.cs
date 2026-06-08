
#region Using directives
using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.HMIProject;
using FTOptix.NativeUI;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.Core;
using FTOptix.NetLogic;
#endregion

public class RuntimeNetLogic1 : BaseNetLogic
{
    private IUAVariable confirmar;
    private IUAVariable email;
    private IUAVariable type;
    private Timer monitoringTimer;
    private bool lastConfirmarValue;
    private bool emailAlreadySent;

    public override void Start()
    {
        confirmar = LogicObject.GetVariable("confirmar");
        email = LogicObject.GetVariable("email");
        type = LogicObject.GetVariable("type");
        try
        {
            monitoringTimer?.Dispose();
            monitoringTimer = null;

            lastConfirmarValue = confirmar?.Value?.Value is bool lastValue
                ? lastValue
                : false;
            emailAlreadySent = false;

            monitoringTimer = new Timer(_ => CheckConfirmarValue(), null, 0, 200);
            Log.Info("RuntimeNetLogic1", "Monitoreo de confirmar iniciado.");
        }
        catch (Exception ex)
        {
            Log.Error("RuntimeNetLogic1", ex.Message);
        }
    }

    public override void Stop()
    {
        try
        {
            monitoringTimer?.Dispose();
            monitoringTimer = null;
        }
        catch (Exception ex)
        {
            Log.Error("RuntimeNetLogic1", ex.Message);
        }
    }

    private void CheckConfirmarValue()
    {
        try
        {
            if (confirmar == null)
            {
                confirmar = LogicObject.GetVariable("confirmar");
            }

            if (confirmar == null)
            {
                return;
            }

            var currentValue = confirmar.Value.Value is bool currentBool
                ? currentBool
                : false;

            if (currentValue && !lastConfirmarValue && !emailAlreadySent)
            {
                SendEmail();
                emailAlreadySent = true;
                Log.Info("RuntimeNetLogic1", "Correo enviado porque confirmar pasó de false a true.");
            }

            if (!currentValue)
            {
                emailAlreadySent = false;
            }

            lastConfirmarValue = currentValue;
        }
        catch (Exception ex)
        {
            Log.Error("RuntimeNetLogic1", ex.Message);
        }
    }

    private void SendEmail()
    {
        var emailValue = email?.Value?.Value?.ToString() ?? string.Empty;
        var typeValue = type?.Value?.Value?.ToString() ?? string.Empty;

        var fromAddress = new MailAddress("diego25baan@gmail.com", "Sender Name");
        var toAddress = new MailAddress("diego25baan@gmail.com", "Destination_Name");
        const string fromPassword = "rgya ncuy ukcf dnou";

        using var smtp = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
        };

        using var message = new MailMessage(fromAddress, toAddress)
        {
            Subject = "Confirmación activada",
            Body = $"{emailValue},{typeValue}"
        };

        smtp.Send(message);
    }
}