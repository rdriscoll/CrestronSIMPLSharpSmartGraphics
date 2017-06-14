// License info and recommendations
//-----------------------------------------------------------------------
// <copyright file="ControlSystem.cs" company="AVPlus Integration Pty Ltd">
//     {c} AV Plus Pty Ltd 2017.
//     http://www.avplus.net.au
//     20170611 Rod Driscoll
//     e: rdriscoll@avplus.net.au
//     m: +61 428 969 608
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//     
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//     
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
//
//     For more details please refer to the LICENSE file located in the root folder 
//      of the project source code;
// </copyright>

namespace AVPlus.SmartGraphics
{
    using System;
    using System.Collections.Generic;
    using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
    using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
    using Crestron.SimplSharpPro.DeviceSupport;         	// For Generic Device Support
    using Crestron.SimplSharp.CrestronIO;                   // For user interfaces
    // using statemenst below require the assembly to be added first.
    // To add assemblies that aren't built in by default right click in Solution Explorer > Project name > References folder and select the reference
    using Crestron.SimplSharpPro.UI;

    using AVPlus.Utils;
    using SG = AVPlus.Utils.UI.SmartGraphicsHelper; // an alias so I don't have to type the whole static class name every time
    using UI = AVPlus.Utils.UI.UserInterfaceHelper;
    
    public class ControlSystem : CrestronControlSystem
    {
        Tsw1060 ui_01;
        XpanelForSmartGraphics ui_02;
        string SgdFileName = "SmartGraphicsDemoXpan.sgd";
        string pin = "1234";
        string keypadText = String.Empty;

        #region join constants

        // Digital joins
        const ushort DIG_TOGGLE_POWER = 1;
        const ushort DIG_MACRO        = 2;
        // Analog joins
        const ushort ANA_BAR_GRAPH = 1;
        const ushort ANA_RANDOM    = 2;
        // Serial joins
        const ushort SER_TITLE = 1;
        const ushort SER_VALUE = 2;
        const ushort SER_INPUT = 3;

        // SmartObject objects
        const ushort SG_BTN_LIST          = 1;
        const ushort SG_DPAD              = 2;
        const ushort SG_KEYPAD            = 3;
        const ushort SG_DYNAMIC_BTN_LIST  = 4;
        const ushort SG_DYNAMIC_ICON_LIST = 5;

        #endregion

        public ControlSystem()
            : base()
        {
            try
            {
                ConfigUserInterfaces();
            }
            catch (Exception e)
            {
                OnDebug(eDebugEventType.Error, "Error in the constructor: {0}", e.Message);
            }
        }

        void ConfigUserInterfaces()
        {
            OnDebug(eDebugEventType.Info, "Configuring UserInterfaces");
            // create the user interfaces that you want in the project
            ui_01 = new Tsw1060(0x03, this);
            ui_02 = new XpanelForSmartGraphics(0x04, this);
            ConfigUserInterface(ui_01);
            ConfigUserInterface(ui_02);
        }
        void ConfigUserInterface(BasicTriListWithSmartObject currentDevice)
        {
            try
            {
                if (typeof(TswX60BaseClass).IsAssignableFrom(currentDevice.GetType()))
                {
                    ((TswX60BaseClass)currentDevice).ExtenderHardButtonReservedSigs.Use(); // must be done before registration. May cause, System.InvalidCastException: InvalidCastException
                    OnDebug(eDebugEventType.Info, "ui_{0:X2} {1} using HardButtonReservedSigs", currentDevice.ID, currentDevice.Name);
                }
                if (currentDevice.Register() == eDeviceRegistrationUnRegistrationResponse.Success)
                    OnDebug(eDebugEventType.Info, "ui_{0:X2} {1} registration success", currentDevice.ID, currentDevice.Name);
                else
                    OnDebug(eDebugEventType.Error, "ui_{0:X2} {1} failed registration. Cause: {2}", currentDevice.ID, currentDevice.Name, currentDevice.RegistrationFailureReason);
                currentDevice.OnlineStatusChange += new OnlineStatusChangeEventHandler(ui_OnlineStatusChange);
                currentDevice.SigChange += new SigEventHandler(ui_SigChange);

                LoadUserInterfaceSmartObjectGraphics(currentDevice);
            }
            catch (Exception e)
            {
                OnDebug(eDebugEventType.Error, "Exception in ConfigUserInterfaces: {0}", e.Message);
            }
        }
        void LoadUserInterfaceSmartObjectGraphics(BasicTriListWithSmartObject currentDevice)
        {
            try
            {
                string location = Directory.GetApplicationDirectory() + "\\" + SgdFileName;
                if (File.Exists(location))
                {
                    currentDevice.LoadSmartObjects(location);
                    OnDebug(eDebugEventType.Info, "{0} SmartObject items loaded", currentDevice.SmartObjects.Count.ToString());
                    foreach (KeyValuePair<uint, SmartObject> kvp in currentDevice.SmartObjects)
                    {
                        kvp.Value.SigChange += new SmartObjectSigChangeEventHandler(SmartObject_SigChange);
                        SG.PrintSmartObjectSigNames(kvp.Value);
                    }
                }
                else
                    OnDebug(eDebugEventType.Info, "SmartObject file{0} does not exist", location);
            }
            catch (Exception e)
            {
                OnDebug(eDebugEventType.Error, "Exception in LoadUserInterfaceSmartObjectGraphics: {0}", e.Message);
            }
        }

        void ui_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            OnDebug(eDebugEventType.Info, "{0} online status {1}", currentDevice.ToString(), args.DeviceOnLine.ToString());
            Type type = currentDevice.GetType();

            SmartObject so = ((BasicTriListWithSmartObject)currentDevice).SmartObjects[SG_DYNAMIC_BTN_LIST];
            int i = 0;
            ushort fontsize = 16;
            string formattedText = UI.FormatTextForUi(currentDevice.Name, fontsize, UI.eCrestronFont.Crestron_Sans_Pro, UI.eNamedColour.White);
            SG.SetSmartObjectVisible(so, ++i, true);
            SG.SetSmartObjectText   (so,   i, formattedText);

            formattedText = String.Format("IPID: 0x{0:X2}", currentDevice.ID);
            formattedText = UI.FormatTextForUi(formattedText, fontsize, UI.eCrestronFont.Crestron_Sans_Pro, UI.eNamedColour.White);
            SG.SetSmartObjectVisible(so, ++i, true);
            SG.SetSmartObjectText   (so,   i, formattedText);

            formattedText = "Type: " + type.Name.ToString();
            formattedText = UI.FormatTextForUi(formattedText, fontsize, UI.eCrestronFont.Crestron_Sans_Pro, UI.eNamedColour.White);
            SG.SetSmartObjectVisible(so, ++i, true);
            SG.SetSmartObjectText   (so,   i, formattedText);
            
            try
            {
                if (typeof(TswFt5Button).IsAssignableFrom(currentDevice.GetType()))
                {
                    formattedText = "IP: " + ((TswFt5Button)currentDevice).ExtenderEthernetReservedSigs.IpAddressFeedback.ToString();
                    formattedText = UI.FormatTextForUi(formattedText, fontsize, UI.eCrestronFont.Crestron_Sans_Pro, UI.eNamedColour.White);
                    SG.SetSmartObjectVisible(so, ++i, true);
                    SG.SetSmartObjectText   (so,   i, formattedText);

                    formattedText = "MAC: " + ((TswFt5Button)currentDevice).ExtenderEthernetReservedSigs.MacAddressFeedback.ToString();
                    formattedText = UI.FormatTextForUi(formattedText, fontsize, UI.eCrestronFont.Crestron_Sans_Pro, UI.eNamedColour.White);
                    SG.SetSmartObjectVisible(so, ++i, true);
                    SG.SetSmartObjectText(so, i, formattedText);
                }

                if (typeof(TswX60BaseClass).IsAssignableFrom(currentDevice.GetType()))
                {
                    ((TswX60BaseClass)currentDevice).ExtenderHardButtonReservedSigs.DeviceExtenderSigChange -= ui_HardButton_SigChange; // remove existing event from invocation list 
                    ((TswX60BaseClass)currentDevice).ExtenderHardButtonReservedSigs.DeviceExtenderSigChange += ui_HardButton_SigChange;
                }
            }
            catch (Exception e)
            {
                OnDebug(eDebugEventType.Info, "ui_OnlineStatusChange exception: {0}", e.Message);
            }
        }

        void ui_HardButton_SigChange(DeviceExtender currentDeviceExtender, SigEventArgs args)
        {
            try
            {
                Sig sig = args.Sig;
                OnDebug(eDebugEventType.Info, "{0} HardButton SigChange type: {1}, sig: {2}, Name: {3}", currentDeviceExtender.ToString(), sig.Type.ToString(), sig.Number.ToString(), sig.Name);
                if (sig.BoolValue) // press
                {
                    OnDebug(eDebugEventType.Info, "Press event on sig number: {0}", sig.Number);
                    switch (sig.Number)
                    {
                        case 1: break;
                    }
                }
                else // release
                {
                }
            }
            catch (Exception e)
            {
                OnDebug(eDebugEventType.Info, "ui_HardButton_SigChange exception: {0}", e.Message);
            }
        }

        void ui_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            Sig sig = args.Sig;
            OnDebug(eDebugEventType.Info, "{0} SigChange in {1} type: {2}, sig: {3}, Name: {4}", currentDevice.ToString(), currentDevice.ID.ToString(), sig.Type.ToString(), sig.Number.ToString(), sig.Name);
            switch (sig.Type)
            {
                case eSigType.Bool:
                    if (sig.BoolValue) // press
                    {
                        OnDebug(eDebugEventType.Info, "Press event on sig number: {0}", sig.Number);
                        switch(sig.Number)
                        {
                            case DIG_TOGGLE_POWER:
                                UI.ToggleDigitalJoin(currentDevice, sig.Number);
                                break;
                            case DIG_MACRO:
                                UI.PulseDigitalJoin(currentDevice, DIG_TOGGLE_POWER);
                                var randomNumber = new Random().Next(ushort.MaxValue);
                                UI.SetAnalogJoin(currentDevice, ANA_RANDOM, (ushort)randomNumber);
                                UI.SetSerialJoin(currentDevice, SER_VALUE, randomNumber.ToString());
                                break;
                        }
                    }
                    else // release
                    {
                    }
                    break;
                case eSigType.UShort:
                    OnDebug(eDebugEventType.Info, "UShortValue: {0}", sig.UShortValue.ToString());
                    switch(sig.Number)
                    {
                        case ANA_BAR_GRAPH:
                            UI.SetAnalogJoin(currentDevice, sig.Number, sig.UShortValue);
                            UI.SetSerialJoin(currentDevice, SER_VALUE, sig.UShortValue.ToString());
                            break;
                        case ANA_RANDOM:
                            UI.SetAnalogJoin(currentDevice, ANA_BAR_GRAPH, sig.UShortValue);
                            break;
                    }
                    break;
                case eSigType.String:
                    OnDebug(eDebugEventType.Info, "StringValue: {0}", sig.StringValue);
                    switch (sig.Number)
                    {
                        case SER_INPUT:
                            UI.SetSerialJoin(currentDevice, SER_VALUE, sig.StringValue.ToString());
                            break;
                    }
                    break;
                default:
                    OnDebug(eDebugEventType.Info, "Unhandled sig type: {0}", sig.Type.ToString());
                    break;
            }
        }

        void SmartObject_SigChange(GenericBase currentDevice, SmartObjectEventArgs args)
        {
            var item = (BasicTriListWithSmartObject)currentDevice;
            SmartObject so = item.SmartObjects[args.SmartObjectArgs.ID];
            Sig sig = args.Sig;
            OnDebug(eDebugEventType.Info, "SmartObject Object ID {0}, on device {1}, type: {2}, Name: {3}, number: {4}", so.ID, so.Device.ID.ToString(), sig.Type.ToString(), sig.Name, sig.Number.ToString());
            switch (args.SmartObjectArgs.ID)
            {
                case SG_DPAD             : SmartObject_DPad_SigChange       (item, args); break;
                case SG_KEYPAD           : SmartObject_KeyPad_SigChange     (item, args); break;
                case SG_BTN_LIST         : SmartObject_BtnList_SigChange    (item, args); break;
                case SG_DYNAMIC_BTN_LIST : SmartObject_DynBtnList_SigChange (item, args); break;
                case SG_DYNAMIC_ICON_LIST: SmartObject_DynIconList_SigChange(item, args); break;
            }
        }

        void SmartObject_KeyPad_SigChange(BasicTriListWithSmartObject currentDevice, SmartObjectEventArgs args)
        {
            if (args.Sig.BoolValue)
            {
                if(args.Sig.Number < 11) // 1 to 9
                    keypadText += args.Sig.Name;
                else if (args.Sig.Number == 11) // MISC_1 - could be anything but we'll make it clear for this example
                    keypadText = "";
                else if (args.Sig.Number == 12) // MISC_2 - could be anything but we'll make it enter for this example
                {
                    keypadText = "PIN " + (keypadText.Equals(pin) ? "Correct": "Wrong");
                    Thread keypad = new Thread(ResetPinTextThread, currentDevice);
                }
                UI.SetSerialJoin(currentDevice, SER_INPUT, keypadText);
            }
            else // release
            {
            }
        }
        object ResetPinTextThread(object o) // not thread safe!
        {
            try
            {
                OnDebug(eDebugEventType.Info, "UResetPinText");
                Thread.Sleep(1000);
                keypadText = "";
                var ui = o as BasicTriList;
                if (ui != null)
                    UI.SetSerialJoin(ui, SER_INPUT, keypadText);
            }
            catch (Exception e)
            {
                OnDebug(eDebugEventType.Info, "ResetPinText exception: {0}", e.Message);
            }
            return null;
        }

        void SmartObject_DPad_SigChange(BasicTriListWithSmartObject currentDevice, SmartObjectEventArgs args) 
        { 
            if (args.Sig.BoolValue)
            {
                switch (args.Sig.Name.ToUpper())
                {
                    case "UP"    : OnDebug(eDebugEventType.Info, "Up pressed"    ); break; // up
                    case "DOWN"  : OnDebug(eDebugEventType.Info, "Down pressed"  ); break; // dn
                    case "LEFT"  : OnDebug(eDebugEventType.Info, "Left pressed"  ); break; // le
                    case "RIGHT" : OnDebug(eDebugEventType.Info, "Right pressed" ); break; // ri
                    case "CENTER": OnDebug(eDebugEventType.Info, "Center pressed"); break; // OK
                    default: 
                        OnDebug(eDebugEventType.Info, "Unhandled keypad button {0} pressed, name:{1}", args.Sig.Number, args.Sig.Name); 
                        break;
                }
            }
            else // release
            {
            }
        }

        void SmartObject_BtnList_SigChange(BasicTriListWithSmartObject currentDevice, SmartObjectEventArgs args)
        {
            SmartObject so = currentDevice.SmartObjects[args.SmartObjectArgs.ID];
            SmartObject soDynBtnList  = currentDevice.SmartObjects[SG_DYNAMIC_BTN_LIST ];
            SmartObject soDynIconList = currentDevice.SmartObjects[SG_DYNAMIC_ICON_LIST];
            Sig sig = args.Sig;
            switch (sig.Type)
            {
                case eSigType.Bool:
                    if (sig.BoolValue)
                    {
                        OnDebug(eDebugEventType.Info, "Press event");
                        switch (sig.Number)
                        {
                            //case 1: break;
                            default:
                                //SG.ToggleSmartObjectVisible(soDynBtnList , (int)sig.Number);       // toggle visibility
                                //SG.ToggleSmartObjectVisible(soDynIconList, (int)sig.Number);       // toggle visibility

                                // toggle the button feedback and put some text onto it
                                string buttonText = "Item " + sig.Number.ToString();
                                SG.ToggleSmartObjectSelected(so, (int)sig.Number); // standard button lists don't support feedback  so this doesn't do anything
                                SG.SetSmartObjectText       (so, (int)sig.Number, buttonText);

                                // soDynBtnList uses dynamic IconAnalogs, of type MediaTransports
                                SG.ToggleSmartObjectVisible(soDynBtnList, (int)sig.Number);       // toggle visibility
                                SG.SetSmartObjectEnabled   (soDynBtnList, (int)sig.Number, true); // enable
                                SG.SetSmartObjectIconAnalog(soDynBtnList, (int)sig.Number, (ushort)sig.Number); // set icon to the next analog

                                // soDynIconList uses dynamic IconSerials, of type IconsLg
                                SG.ToggleSmartObjectVisible(soDynIconList, (int)sig.Number);       // toggle visibility
                                SG.SetSmartObjectEnabled   (soDynIconList, (int)sig.Number, true); // enable
                                string icon =  UI.IconsLgDict[0];
                                SG.SetSmartObjectIconSerial(soDynIconList, (int)sig.Number, icon); // set icon to the next serial
                                break;
                        }
                    }
                    else
                    {
                        OnDebug(eDebugEventType.Info, "Release event");
                    }
                    break;
                case eSigType.UShort:
                    OnDebug(eDebugEventType.Info, "UShortValue: {0}", sig.UShortValue.ToString());
                    break;
                case eSigType.String:
                    OnDebug(eDebugEventType.Info, "StringValue: {0}", sig.StringValue);
                    break;
                default:
                    OnDebug(eDebugEventType.Info, "Unhandled sig type: {0}", sig.Type.ToString());
                    break;
            }
        }
        void SmartObject_DynBtnList_SigChange(BasicTriListWithSmartObject currentDevice, SmartObjectEventArgs args)
        {
            SmartObject so = currentDevice.SmartObjects[args.SmartObjectArgs.ID];
            SmartObject soBtnList     = currentDevice.SmartObjects[SG_BTN_LIST];
            SmartObject soDynIconList = currentDevice.SmartObjects[SG_DYNAMIC_ICON_LIST];
            Sig sig = args.Sig;
            switch (sig.Type)
            {
                case eSigType.Bool:
                    if (sig.BoolValue)
                    {
                        OnDebug(eDebugEventType.Info, "Press event");
                        switch (sig.Number)
                        {
                            default:
                                int number = AVPlus.Utils.StringHelper.Atoi(sig.Name); // Number is offset by 10 so we need item with no offset
                                // toggle the button feedback and put some text onto it
                                SG.ToggleSmartObjectDigitalJoin(so, (int)sig.Number);
                                string buttonText = "Item " + sig.Number.ToString() + " " + SG.GetSmartObjectDigitalJoin(so, (int)sig.Number).ToString();
                                string formattedText = UI.FormatTextForUi(buttonText, 20, UI.eCrestronFont.Crestron_Sans_Pro, UI.eNamedColour.White);
                                SG.SetSmartObjectText(so, number, formattedText);

                                SG.ToggleSmartObjectEnabled(soDynIconList, number);       // enable
                                break;
                        }
                    }
                    else
                    {
                        OnDebug(eDebugEventType.Info, "Release event");
                    }
                    break;
                case eSigType.UShort:
                    OnDebug(eDebugEventType.Info, "UShortValue: {0}", sig.UShortValue.ToString());
                    break;
                case eSigType.String:
                    OnDebug(eDebugEventType.Info, "StringValue: {0}", sig.StringValue);
                    break;
                default:
                    OnDebug(eDebugEventType.Info, "Unhandled sig type: {0}", sig.Type.ToString());
                    break;
            }
        }
        void SmartObject_DynIconList_SigChange(BasicTriListWithSmartObject currentDevice, SmartObjectEventArgs args)
        {
            SmartObject so = currentDevice.SmartObjects[args.SmartObjectArgs.ID];
            SmartObject soBtnList     = currentDevice.SmartObjects[SG_BTN_LIST];
            SmartObject soDynBtnList  = currentDevice.SmartObjects[SG_DYNAMIC_BTN_LIST];
            Sig sig = args.Sig;
            switch (sig.Type)
            {
                case eSigType.Bool:
                    if (sig.BoolValue)
                    {
                        OnDebug(eDebugEventType.Info, "Press event");
                        switch (sig.Number)
                        {
                            default:
                                int number = StringHelper.Atoi(sig.Name); // Number is offset by 10 so we need item with no offset
                                // toggle the button feedback and put some text onto it
//                                SG.ToggleSmartObjectDigitalJoin(so, (int)sig.Number);
                                string buttonText = "Item " + sig.Number.ToString() + " " + SG.GetSmartObjectDigitalJoin(so, (int)sig.Number).ToString();
                                //SG.SetSmartObjectText        (so, (int)sig.Number, buttonText);
//                                SG.SetSmartObjectText          (so, number, buttonText);
                                string icon = UI.IconsLgDict[(ushort)number];
                                SG.SetSmartObjectIconSerial    (so, number, icon); // set icon to the next serial

                                //SG.ToggleSmartObjectEnabled(soDynBtnList, number);       // enable

                                //SG.ToggleSmartObjectDigitalJoin(soBtnList, number);
                                //SG.SetSmartObjectText          (soBtnList, number, buttonText);
                                break;
                        }
                    }
                    else
                    {
                        OnDebug(eDebugEventType.Info, "Release event");
                    }
                    break;
                case eSigType.UShort:
                    OnDebug(eDebugEventType.Info, "UShortValue: {0}", sig.UShortValue.ToString());
                    break;
                case eSigType.String:
                    OnDebug(eDebugEventType.Info, "StringValue: {0}", sig.StringValue);
                    break;
                default:
                    OnDebug(eDebugEventType.Info, "Unhandled sig type: {0}", sig.Type.ToString());
                    break;
            }
        }

        void OnDebug(eDebugEventType eventType, string str, params object[] list)
        {
            Logging.OnDebug(eventType, str, list);
        }

    }
}