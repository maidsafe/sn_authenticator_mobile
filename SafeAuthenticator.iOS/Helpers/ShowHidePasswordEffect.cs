// Copyright 2020 MaidSafe.net limited.
//
// This SAFE Network Software is licensed to you under the MIT license <LICENSE-MIT
// http://opensource.org/licenses/MIT> or the Modified BSD license <LICENSE-BSD
// https://opensource.org/licenses/BSD-3-Clause>, at your option. This file may not be copied,
// modified, or distributed except according to those terms. Please review the Licences for the
// specific language governing permissions and limitations relating to use of the SAFE Network
// Software.

﻿using System.Drawing;
using SafeAuthenticator.Controls;
using SafeAuthenticator.iOS.Helpers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportEffect(typeof(ShowHidePasswordEffect), "ShowHidePasswordEffect")]

namespace SafeAuthenticator.iOS.Helpers
{
    public class ShowHidePasswordEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            ConfigureControl();
        }

        protected override void OnDetached()
        {
        }

        private void ConfigureControl()
        {
            if (Control != null)
            {
                UITextField vUpdatedEntry = (UITextField)Control;
                var buttonRect = UIButton.FromType(UIButtonType.Custom);
                buttonRect.SetImage(new UIImage("ShowPasswordIcon"), UIControlState.Normal);
                buttonRect.TouchUpInside += (sender, e1) =>
                {
                    if (vUpdatedEntry.SecureTextEntry)
                    {
                        vUpdatedEntry.SecureTextEntry = false;
                        buttonRect.SetImage(new UIImage("HidePasswordIcon"), UIControlState.Normal);
                    }
                    else
                    {
                        vUpdatedEntry.SecureTextEntry = true;
                        buttonRect.SetImage(new UIImage("ShowPasswordIcon"), UIControlState.Normal);
                    }
                };

                buttonRect.Frame = ((MaterialEntry)Element).ErrorDisplay !=
                                   ErrorDisplay.None
                    ? new CoreGraphics.CGRect(5.0f, -4.0f, 25.0f, 25.0f)
                    : new CoreGraphics.CGRect(5.0f, 5.0f, 25.0f, 25.0f);
                buttonRect.ContentMode = UIViewContentMode.Right;

                var paddingViewRight = new UIView(new RectangleF(5.0f, -5.0f, 30.0f, 30.0f))
                {
                    buttonRect
                };
                paddingViewRight.ContentMode = UIViewContentMode.BottomRight;

                vUpdatedEntry.RightView = paddingViewRight;
                vUpdatedEntry.RightViewMode = UITextFieldViewMode.Always;

                Control.Layer.CornerRadius = 4;
                Control.Layer.BorderColor = new CoreGraphics.CGColor(255, 255, 255);
                Control.Layer.MasksToBounds = true;
                vUpdatedEntry.TextAlignment = UITextAlignment.Left;
            }
        }
    }
}
