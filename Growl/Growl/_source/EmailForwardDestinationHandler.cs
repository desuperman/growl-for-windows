using System;
using System.Collections.Generic;
using System.Text;

namespace Growl
{
    public class EmailForwardDestinationHandler : IForwardDestinationHandler
    {
        #region IForwardDestinationHandler Members

        public List<Type> Register()
        {
            List<Type> list = new List<Type>();
            list.Add(typeof(EmailForwardDestination));
            return list;
        }

        public Growl.UI.ForwardDestinationSettingsPanel GetSettingsPanel(ForwardDestination fd)
        {
            return new Growl.UI.EmailForwardInputs();
        }

        public Growl.UI.ForwardDestinationSettingsPanel GetSettingsPanel(ForwardDestinationListItem fdli)
        {
            return new Growl.UI.EmailForwardInputs();
        }

        public List<ForwardDestinationListItem> GetListItems()
        {
            ForwardDestinationListItem item = new ForwardDestinationListItem(Properties.Resources.AddComputer_AddEmail, ForwardDestinationPlatformType.Email.Icon, this);
            List<ForwardDestinationListItem> list = new List<ForwardDestinationListItem>();
            list.Add(item);
            return list;
        }

        #endregion
    }
}