using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Net.Mail;
using System.IO;

namespace HomeOS.Shared
{
    [DataContract]
    public class EmailRequestInfo : JsonSerializerBase<EmailRequestInfo>
    {
        private List<Attachment> _attachments;

        [DataMember(Name = "SmtpUser")]
        public string SmtpUser { get; set; }
        [DataMember(Name = "SmtpPassword")]
        public string SmtpPassword { get; set; }
        [DataMember(Name = "SmtpServer")]
        public string SmtpServer { get; set; }
        [DataMember(Name = "Destination")]
        public string Destination { get; set; }
        [DataMember(Name = "Subject")]
        public String Subject { get; set; }
        [DataMember(Name = "Body")]
        public string Body { get; set; }
        [DataMember(Name = "Attachments")]
        public List<Attachment> Attachments  
        {
            get
            {
                bool genDecodedAttachments = false;
                List<Attachment> attachmentsLocal;
                for (int i = 0; i < _attachments.Count; ++i)
                {
                    if (null == _attachments[i].ContentStream )
                    {
                        genDecodedAttachments = true;
                        break;
                    }
                }

                if (genDecodedAttachments)
                {
                    attachmentsLocal = new List<Attachment>();
                    for (int i = 0; i < _attachments.Count; ++i)
                    {
                        attachmentsLocal.Add( new Attachment(
                                                new MemoryStream(Convert.FromBase64String(AttachmentsEncodedStreams[i])),
                                                _attachments[i].Name,
                                                _attachments[i].ContentType.MediaType
                                                )
                                             );
                    }

                    this._attachments = attachmentsLocal;
                }

                return this._attachments;
            }
            private set 
            { 
                _attachments = value;
            }
        }
        [DataMember(Name = "AttachmentsEncodedStreams")]
        private List<string> AttachmentsEncodedStreams { get; set; }

        public EmailRequestInfo(string smtpUser, string smtpPassword, string smtpServer, string dest, string subject, string body, List<Attachment> attachments)
        {
            this.SmtpUser = smtpUser;
            this.SmtpPassword = smtpPassword;
            this.SmtpServer = smtpServer;
            this.Destination = dest;
            this.Subject = subject;
            this.Body = body;
            this._attachments = attachments;
            this.AttachmentsEncodedStreams = new List<string>();
            foreach (Attachment attach in _attachments)
            {
                MemoryStream memStream = new MemoryStream();
                attach.ContentStream.CopyTo(memStream);
                this.AttachmentsEncodedStreams.Add(Convert.ToBase64String(memStream.GetBuffer()));
            }
        }

        public override string ToString()
        {
            int cAttachments =0;
            long bytesAttachTotal = 0;
            if (null != _attachments)
            {
                cAttachments = _attachments.Count;
                for (int i = 0; i < cAttachments; ++i)
                {
                    if (null != _attachments[i].ContentStream)
                    {
                        bytesAttachTotal += _attachments[i].ContentStream.Length;
                    }
                }
            }
            return string.Format("SmtpUser: {0} SmtpPassword: {1} SmtpServer: {2} Destination: {3} Subject: {4} Body: {5}, Attachments Count: {6}, Attachments Total Size: {7}", 
                                 SmtpUser, SmtpPassword, SmtpServer, Destination, Subject, Body, cAttachments, bytesAttachTotal);
        }
    }

    public enum EmailSendStatus
    {
        SentSuccessfully = 1,
        SendFailure = 2
    };


    [DataContract]
    public class EmailStatus : JsonSerializerBase<EmailStatus>
    {
        [DataMember(Name = "EmailSendStatus")]
        public EmailSendStatus SendStatus { get; set; }
        [DataMember(Name = "SendFailureMessage")]
        public string SendFailureMessage { get; set; }
    }

}
