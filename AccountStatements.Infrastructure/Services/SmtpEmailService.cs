using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AccountStatements.Application.DTOs;
using AccountStatements.Application.Interfaces;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace AccountStatements.Infrastructure.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendStatementEmailAsync(string recipientEmail, string recipientName, AccountStatementDto statement, CancellationToken cancellationToken = default)
        {
            try
            {
                var smtpSection = _configuration.GetSection("SmtpSettings");
                var server = smtpSection["Server"] ?? "smtp.gmail.com";
                var port = int.Parse(smtpSection["Port"] ?? "465");
                var senderName = smtpSection["SenderName"] ?? "Account Statements";
                var senderEmail = smtpSection["SenderEmail"] ?? "";
                var username = smtpSection["Username"] ?? "";
                var password = smtpSection["Password"] ?? "";

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(new MailboxAddress(recipientName, recipientEmail));
                message.Subject = $"Your Account Statement for {statement.StatementMonth}";

                var bodyBuilder = new BodyBuilder();

                var sb = new StringBuilder();
                sb.AppendLine("<!DOCTYPE html>");
                sb.AppendLine("<html>");
                sb.AppendLine("<head>");
                sb.AppendLine("  <meta charset='utf-8'>");
                sb.AppendLine("  <style>");
                sb.AppendLine("    body { font-family: 'Segoe UI', Arial, sans-serif; color: #333333; line-height: 1.6; margin: 0; padding: 20px; background-color: #f7f9fc; }");
                sb.AppendLine("    .container { max-width: 600px; margin: 0 auto; background: #ffffff; padding: 30px; border-radius: 8px; box-shadow: 0 4px 10px rgba(0,0,0,0.05); border-top: 5px solid #0056b3; }");
                sb.AppendLine("    h2 { color: #0056b3; margin-top: 0; border-bottom: 2px solid #eaeaea; padding-bottom: 10px; }");
                sb.AppendLine("    .details-table, .transactions-table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
                sb.AppendLine("    .details-table td { padding: 8px 0; vertical-align: top; }");
                sb.AppendLine("    .details-table td.label { font-weight: bold; color: #555555; width: 150px; }");
                sb.AppendLine("    .transactions-table th { background-color: #f2f5fa; color: #444444; font-weight: bold; text-align: left; padding: 10px; border-bottom: 2px solid #eaeaea; }");
                sb.AppendLine("    .transactions-table td { padding: 10px; border-bottom: 1px solid #eeeeee; }");
                sb.AppendLine("    .amount-positive { color: #2e7d32; font-weight: bold; text-align: right; }");
                sb.AppendLine("    .amount-negative { color: #c62828; font-weight: bold; text-align: right; }");
                sb.AppendLine("    .footer { margin-top: 30px; font-size: 0.85em; color: #888888; text-align: center; border-top: 1px solid #eaeaea; padding-top: 15px; }");
                sb.AppendLine("  </style>");
                sb.AppendLine("</head>");
                sb.AppendLine("<body>");
                sb.AppendLine("  <div class='container'>");
                sb.AppendLine("    <h2>Monthly Account Statement</h2>");
                sb.AppendLine($"    <p>Dear {recipientName},</p>");
                sb.AppendLine($"    <p>Please find below your account activity statement for the month of <strong>{statement.StatementMonth}</strong>.</p>");
                
                sb.AppendLine("    <table class='details-table'>");
                sb.AppendLine($"      <tr><td class='label'>Statement Month:</td><td>{statement.StatementMonth}</td></tr>");
                sb.AppendLine($"      <tr><td class='label'>Generated At:</td><td>{statement.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC</td></tr>");
                sb.AppendLine($"      <tr><td class='label'>Starting Balance:</td><td style='font-size: 1.1em; font-weight: bold;'>${statement.StartingBalance:N2}</td></tr>");
                sb.AppendLine($"      <tr><td class='label'>Ending Balance:</td><td style='font-size: 1.1em; font-weight: bold; color: #0056b3;'>${statement.EndingBalance:N2}</td></tr>");
                sb.AppendLine("    </table>");

                sb.AppendLine("    <h3>Transaction Activity</h3>");
                sb.AppendLine("    <table class='transactions-table'>");
                sb.AppendLine("      <thead>");
                bodyBuilder.HtmlBody = sb.ToString();
                sb.AppendLine("        <tr>");
                sb.AppendLine("          <th>Date</th>");
                sb.AppendLine("          <th>Description</th>");
                sb.AppendLine("          <th style='text-align: right;'>Amount</th>");
                sb.AppendLine("        </tr>");
                sb.AppendLine("      </thead>");
                sb.AppendLine("      <tbody>");

                if (statement.Transactions == null || statement.Transactions.Count == 0)
                {
                    sb.AppendLine("        <tr>");
                    sb.AppendLine("          <td colspan='3' style='text-align: center; color: #888888;'>No transactions recorded for this period.</td>");
                    bodyBuilder.HtmlBody = sb.ToString();
                    sb.AppendLine("        </tr>");
                }
                else
                {
                    foreach (var t in statement.Transactions)
                    {
                        var amtClass = t.Amount >= 0 ? "amount-positive" : "amount-negative";
                        var amtSign = t.Amount >= 0 ? "+" : "";
                        sb.AppendLine("        <tr>");
                        sb.AppendLine($"          <td>{t.TransactionDate:yyyy-MM-dd}</td>");
                        sb.AppendLine($"          <td>{t.Description}</td>");
                        sb.AppendLine($"          <td class='{amtClass}'>{amtSign}${Math.Abs(t.Amount):N2}</td>");
                        sb.AppendLine("        </tr>");
                    }
                }

                sb.AppendLine("      </tbody>");
                sb.AppendLine("    </table>");
                sb.AppendLine("  </div>");
                sb.AppendLine("</body>");
                sb.AppendLine("</html>");

                bodyBuilder.HtmlBody = sb.ToString();
                message.Body = bodyBuilder.ToMessageBody();

                SecureSocketOptions socketOption;
                if (port == 465)
                {
                    socketOption = SecureSocketOptions.SslOnConnect;
                }
                else if (port == 587)
                {
                    socketOption = SecureSocketOptions.StartTls;
                }
                else
                {
                    socketOption = SecureSocketOptions.Auto;
                }

                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    await client.ConnectAsync(server, port, socketOption, cancellationToken);
                    await client.AuthenticateAsync(username, password, cancellationToken);
                    await client.SendAsync(message, cancellationToken);
                    await client.DisconnectAsync(true, cancellationToken);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send monthly statement email to {Email}", recipientEmail);
                return false;
            }
        }
    }
}
