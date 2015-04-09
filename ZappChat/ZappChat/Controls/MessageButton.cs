﻿using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace ZappChat.Controls
{
    [TemplateVisualState(Name = "NoUnreadMessages", GroupName = "MessageView"),
    TemplateVisualState(Name = "UnreadMessages", GroupName = "MessageView")]
    public class MessageButton : Button
    {
        public static readonly DependencyProperty MessagesCountProperty =
            DependencyProperty.Register("MessagesCount", typeof (string), typeof (MessageButton));

        public string MessagesCount
        {
            get { return GetValue(MessagesCountProperty) as string; }
            set { SetValue(MessagesCountProperty, SetMessagesCount(int.Parse(value))); }
        }
        static MessageButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MessageButton), new FrameworkPropertyMetadata(typeof(MessageButton)));
        }

        public void SetStartupState()
        {
            SetMessagesCount(0);
        }

        public string SetMessagesCount(int number)
        {
            if (number <= 0)
            {
                VisualStateManager.GoToState(this, "NoUnreadMessages", true);
                return "";
            }
            if(number >= 100)
            {
                VisualStateManager.GoToState(this, "UnreadMessages", true);
                return "99+";
            }
            VisualStateManager.GoToState(this, "UnreadMessages", true);
            return number.ToString(CultureInfo.InvariantCulture);
        }
    }
}
