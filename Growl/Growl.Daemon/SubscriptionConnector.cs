using System;
using System.Collections.Generic;
using System.Text;
using Growl.Connector;

namespace Growl.Daemon
{
    /// <summary>
    /// Used to communicate with Growl when subscribing to notifications.
    /// </summary>
    /// <remarks>
    /// After calling the <see cref="Subscribe"/> method, this class will
    /// automatically try to renew the subscription at regular intervals in
    /// order to keep the subscription alive.
    /// The renewal interval is determined by the subscribed-to server.
    /// </remarks>
    public class SubscriptionConnector : ConnectorBase
    {
        /// <summary>
        /// Represents methods that handle responses to 'SUBSCRIBE' requests
        /// </summary>
        public delegate void ResponseEventHandler(SubscriptionResponse response);

        /// <summary>
        /// Occurs when an 'OK' response is received
        /// </summary>
        public event ResponseEventHandler OKResponse;

        /// <summary>
        /// Occurs when an 'ERROR' response is received
        /// </summary>
        public event ResponseEventHandler ErrorResponse;

        /// <summary>
        /// The <see cref="Subscriber"/> information
        /// </summary>
        private Subscriber subscriber;

        /// <summary>
        /// The interval at which renewal requests are sent.
        /// </summary>
        private int ttl;

        /// <summary>
        /// Fires more frequently than the TTL value in order to keep the subscription alive.
        /// </summary>
        private System.Timers.Timer timer;



        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionConnector"/> class.
        /// </summary>
        /// <param name="subscriber">The <see cref="Subscriber"/> information/</param>
        /// <param name="password">The password used to authenticate requests</param>
        /// <param name="hostname">The hostname of the Growl instance to subscribe to.</param>
        public SubscriptionConnector(Subscriber subscriber, string password, string hostname)
            : this(subscriber, password, hostname, ConnectorBase.TCP_PORT)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionConnector"/> class
        /// and allows the communication port to be set.
        /// </summary>
        /// <param name="subscriber">The <see cref="Subscriber"/> information/</param>
        /// <param name="password">The password used to authenticate requests</param>
        /// <param name="hostname">The hostname of the Growl instance to subscribe to.</param>
        /// <param name="port">The port of the Growl instance to subscribe to.</param>
        public SubscriptionConnector(Subscriber subscriber, string password, string hostname, int port)
            : base(password, hostname, port)
        {
            this.subscriber = subscriber;

            this.timer = new System.Timers.Timer();
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
        }

        /// <summary>
        /// Subscribes the caller to be notified whenever the server receives a notification.
        /// </summary>
        /// <remarks>
        /// Once subscribed, this class will handle renewing the subscription automatically
        /// for the lifetime of the instance.
        /// 
        /// IMPORTANT: If the subscription succeeds (OnOKResponse event fires), the calling code should
        /// make sure to add the subscribed-to server's password to their PasswordManager instance.
        /// The subscribed-to server will be using its own password to authorize/encrypt requests
        /// (since it never knows the subscriber's password), so this password should be authorized
        /// for the lifetime of the subscription. // TODO: should we manage this for them (maybe pass in PasswordManager instance to Subscribe?)
        /// </remarks>
        public virtual void Subscribe()
        {
            RenewSubscription();
        }

        /// <summary>
        /// Stops renewing the subscription.
        /// </summary>
        /// <remarks>
        /// Note that 'StopRenewing' is not necessarily the same as 'Unsubscribe'. If the subscription password is still
        /// included in the PasswordManager and the subscribed-to server has not reach the expiration TTL yet, notifications
        /// will still be forwarded and received.
        /// In order to stop receiving notifications completely, you must also remove the subscription password from
        /// the PasswordManager.
        /// </remarks>
        public void StopRenewing()
        {
            StopRenewalTimer();
        }

        /// <summary>
        /// Renews the callers subscription to the server to avoid being timed-out.
        /// </summary>
        /// <remarks>
        /// By default, the renewal interval is equal to (Server TTL - 30 seconds).
        /// </remarks>
        private void RenewSubscription()
        {
            HeaderCollection headers = this.subscriber.ToHeaders();

            MessageBuilder mb = new MessageBuilder(RequestType.SUBSCRIBE, this.GetKey());
            foreach (Header header in headers)
            {
                mb.AddHeader(header);
            }

            Send(mb, OnResponseReceived, false);
        }

        /// <summary>
        /// Parses the response and raises the appropriate event
        /// </summary>
        /// <param name="responseText">The raw GNTP response</param>
        protected override void OnResponseReceived(string responseText)
        {
            CallbackData cd;
            HeaderCollection headers;
            MessageParser mp = new MessageParser();
            Response response = mp.Parse(responseText, out cd, out headers);

            SubscriptionResponse sr = SubscriptionResponse.FromResponse(response, headers);

            if (sr.IsOK)
            {
                this.ttl = sr.TTL - 30;  // try to renew 30 seconds before the server disconnects us

                if (this.ttl > 0) StartRenewalTimer();
                else StopRenewalTimer();

                this.OnOKResponse(sr);
            }
            else
            {
                StopRenewalTimer();
                this.OnErrorResponse(sr);
            }
        }

        /// <summary>
        /// Occurs when any of the following network conditions occur:
        /// 1. Unable to connect to target host for any reason
        /// 2. Write request fails
        /// 3. Read request fails
        /// </summary>
        /// <param name="response">The <see cref="Response"/> that contains information about the failure</param>
        protected override void OnCommunicationFailure(Response response)
        {
            SubscriptionResponse sr = SubscriptionResponse.FromResponse(response, null);
            this.OnErrorResponse(sr);
        }

        /// <summary>
        /// Called when an 'OK' response occurs.
        /// </summary>
        /// <param name="response">The <see cref="Response"/></param>
        protected void OnOKResponse(SubscriptionResponse response)
        {
            if (this.OKResponse != null)
            {
                this.OKResponse(response);
            }
        }

        /// <summary>
        /// Called when an 'ERROR' response occurs.
        /// </summary>
        /// <param name="response">The <see cref="Response"/></param>
        protected void OnErrorResponse(SubscriptionResponse response)
        {
            if (this.ErrorResponse != null)
            {
                this.ErrorResponse(response);
            }
        }

        /// <summary>
        /// Starts the renewal timer.
        /// </summary>
        private void StartRenewalTimer()
        {
            this.timer.Interval = (this.ttl * 1000);
            this.timer.Start();
        }

        /// <summary>
        /// Stops the renewal timer.
        /// </summary>
        private void StopRenewalTimer()
        {
            this.timer.Stop();
        }

        /// <summary>
        /// Fires when the renewal timer elapses. Renews the caller's subscription.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Event args</param>
        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            RenewSubscription();
        }
    }
}
