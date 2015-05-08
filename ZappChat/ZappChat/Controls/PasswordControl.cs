﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ZappChat.Controls
{
    [TemplatePart(Name = "ShowPasswordButton", Type = typeof(ToggleButton)),
     TemplatePart(Name = "Password", Type = typeof(PasswordBox)),
     TemplatePart(Name = "Text", Type = typeof(TextBox))]
    public class PasswordControl : Control
    {
        private PasswordBox _password;
        private TextBox _text;
        private ToggleButton _showButton;
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius",
            typeof (CornerRadius), typeof (PasswordControl));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius) GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty ViewPasswordProperty = DependencyProperty.Register("ViewPassword", typeof (bool),
            typeof (PasswordControl), new FrameworkPropertyMetadata(false));

        public bool ViewPassword
        {
            get { return (bool) GetValue(ViewPasswordProperty); }
            set { SetValue(ViewPasswordProperty, value); }
        }

        public static readonly DependencyProperty SecretProperty = DependencyProperty.Register("Secret", typeof (string),
            typeof (PasswordControl), new FrameworkPropertyMetadata(""));

        private string Secret
        {
            get { return GetValue(SecretProperty) as string; }
            set { SetValue(SecretProperty, value); }
        }
        static PasswordControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (PasswordControl),
                new FrameworkPropertyMetadata(typeof (PasswordControl)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _password = GetTemplateChild("Password") as PasswordBox;
            _showButton = GetTemplateChild("ShowPasswordButton") as ToggleButton;
            _text = GetTemplateChild("Text") as TextBox;
            _text.GotKeyboardFocus += (sender, args) =>
            {
                if (!ViewPassword)
                {
                    _text.Visibility = Visibility.Collapsed;
                    _password.Visibility = Visibility.Visible;
                    Keyboard.Focus(_password);
                }
            };
            _password.LostKeyboardFocus += (sender, args) =>
            {
                if (_password.Password == string.Empty)
                {
                    _text.Visibility = Visibility.Visible;
                    _password.Visibility = Visibility.Collapsed;
                    _text.Text = "Пароль";
                }
            };
            _showButton.Checked += (sender, args) =>
            {
                Secret = _password.Password;
                ViewPassword = true;
                _text.Visibility = Visibility.Visible;
                _password.Visibility = Visibility.Collapsed;
                _text.Foreground = Brushes.Black;
                _text.Text = Secret;
            };
            _showButton.Unchecked += (sender, args) =>
            {
                Secret = _text.Text;
                ViewPassword = false;
                if (Secret == "")
                {
                    _text.Text = "Пароль";
                }
                else
                {
                    _text.Visibility = Visibility.Collapsed;
                    _password.Visibility = Visibility.Visible;
                }
                _text.Foreground = new SolidColorBrush(Color.FromRgb(141, 141, 141));
                _password.Password = Secret;
            };
            
        }

        public string GetPassword()
        {
            return _text.Visibility == Visibility.Visible && _text.Text != "Пароль" ? _text.Text : _password.Password;
        }
    }
}
