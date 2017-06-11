using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.DeviceSupport;         	// For Generic Device Support
using Crestron.SimplSharp.CrestronIO;                   // For user interfaces
// using statemenst below require the assembly to be added first.
// To add assemblies that aren't built in by default right click in Solution Explorer > Project name > References folder and select the reference
using Crestron.SimplSharpPro.UI;

using SG = AVPlus.Utils.UI.SmartGraphicsHelper; // an alias so I don't have to type the whole static class name every time
using UI = AVPlus.Utils.UI.UserInterfaceHelper;

namespace AVPlus
{
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
                StringHelper.OnDebug(eDebugEventType.Error, "Error in the constructor: {0}", e.Message);
            }
        }

        void ConfigUserInterfaces()
        {
            StringHelper.OnDebug(eDebugEventType.Info, "Configuring UserInterfaces");
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
                    StringHelper.OnDebug(eDebugEventType.Info, "ui_{0:X2} {1} using HardButtonReservedSigs", currentDevice.ID, currentDevice.Name);
                }
                if (currentDevice.Register() == eDeviceRegistrationUnRegistrationResponse.Success)
                    StringHelper.OnDebug(eDebugEventType.Info, "ui_{0:X2} {1} registration success", currentDevice.ID, currentDevice.Name);
                else
                    StringHelper.OnDebug(eDebugEventType.Error, "ui_{0:X2} {1} failed registration. Cause: {2}", currentDevice.ID, currentDevice.Name, currentDevice.RegistrationFailureReason);
                currentDevice.OnlineStatusChange += new OnlineStatusChangeEventHandler(ui_OnlineStatusChange);
                currentDevice.SigChange += new SigEventHandler(ui_SigChange);

                LoadUserInterfaceSmartObjectGraphics(currentDevice);
            }
            catch (Exception e)
            {
                StringHelper.OnDebug(eDebugEventType.Error, "Exception in ConfigUserInterfaces: {0}", e.Message);
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
                    StringHelper.OnDebug(eDebugEventType.Info, "{0} SmartObject items loaded", currentDevice.SmartObjects.Count.ToString());
                    foreach (KeyValuePair<uint, SmartObject> kvp in currentDevice.SmartObjects)
                    {
                        kvp.Value.SigChange += new SmartObjectSigChangeEventHandler(SmartObject_SigChange);
                        SG.PrintSmartObjectSigNames(kvp.Value);
                    }
                }
                else
                    StringHelper.OnDebug(eDebugEventType.Info, "SmartObject file{0} does not exist", location);
            }
            catch (Exception e)
            {
                StringHelper.OnDebug(eDebugEventType.Error, "Exception in LoadUserInterfaceSmartObjectGraphics: {0}", e.Message);
            }
        }

        void ui_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            StringHelper.OnDebug(eDebugEventType.Info, "{0} online status {1}", currentDevice.ToString(), args.DeviceOnLine.ToString());
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
                StringHelper.OnDebug(eDebugEventType.Info, "ui_OnlineStatusChange exception: {0}", e.Message);
            }
        }

        void ui_HardButton_SigChange(DeviceExtender currentDeviceExtender, SigEventArgs args)
        {
            try
            {
                Sig sig = args.Sig;
                StringHelper.OnDebug(eDebugEventType.Info, "{0} HardButton SigChange type: {1}, sig: {2}, Name: {3}", currentDeviceExtender.ToString(), sig.Type.ToString(), sig.Number.ToString(), sig.Name);
                if (sig.BoolValue) // press
                {
                    StringHelper.OnDebug(eDebugEventType.Info, "Press event on sig number: {0}", sig.Number);
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
                StringHelper.OnDebug(eDebugEventType.Info, "ui_HardButton_SigChange exception: {0}", e.Message);
            }
        }

        void ui_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            Sig sig = args.Sig;
            StringHelper.OnDebug(eDebugEventType.Info, "{0} SigChange in {1} type: {2}, sig: {3}, Name: {4}", currentDevice.ToString(), currentDevice.ID.ToString(), sig.Type.ToString(), sig.Number.ToString(), sig.Name);
            switch (sig.Type)
            {
                case eSigType.Bool:
                    if (sig.BoolValue) // press
                    {
                        StringHelper.OnDebug(eDebugEventType.Info, "Press event on sig number: {0}", sig.Number);
                        switch(sig.Number)
                        {
                            case DIG_TOGGLE_POWER:
                                ToggleDigitalJoin(currentDevice, sig.Number);
                                break;
                            case DIG_MACRO:
                                PulseDigitalJoin(currentDevice, DIG_TOGGLE_POWER);
                                var randomNumber = new Random().Next(ushort.MaxValue);
                                SetAnalogJoin(currentDevice, ANA_RANDOM, (ushort)randomNumber);
                                SetSerialJoin(currentDevice, SER_VALUE, randomNumber.ToString());
                                break;
                        }
                    }
                    else // release
                    {
                    }
                    break;
                case eSigType.UShort:
                    StringHelper.OnDebug(eDebugEventType.Info, "UShortValue: {0}", sig.UShortValue.ToString());
                    switch(sig.Number)
                    {
                        case ANA_BAR_GRAPH:
                            SetAnalogJoin(currentDevice, sig.Number, sig.UShortValue);
                            SetSerialJoin(currentDevice, SER_VALUE , sig.UShortValue.ToString());
                            break;
                        case ANA_RANDOM:
                            SetAnalogJoin(currentDevice, ANA_BAR_GRAPH, sig.UShortValue);
                            break;
                    }
                    break;
                case eSigType.String:
                    StringHelper.OnDebug(eDebugEventType.Info, "StringValue: {0}", sig.StringValue);
                    switch (sig.Number)
                    {
                        case SER_INPUT:
                            SetSerialJoin(currentDevice, SER_VALUE, sig.StringValue.ToString());
                            break;
                    }
                    break;
                default:
                    StringHelper.OnDebug(eDebugEventType.Info, "Unhandled sig type: {0}", sig.Type.ToString());
                    break;
            }
        }

        void SmartObject_SigChange(GenericBase currentDevice, SmartObjectEventArgs args)
        {
            var item = (BasicTriListWithSmartObject)currentDevice;
            SmartObject so = item.SmartObjects[args.SmartObjectArgs.ID];
            Sig sig = args.Sig;
            StringHelper.OnDebug(eDebugEventType.Info, "SmartObject Object ID {0}, on device {1}, type: {2}, Name: {3}, number: {4}", so.ID, so.Device.ID.ToString(), sig.Type.ToString(), sig.Name, sig.Number.ToString());
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
                SetSerialJoin(currentDevice, SER_INPUT, keypadText);
            }
            else // release
            {
            }
        }
        object ResetPinTextThread(object o) // not thread safe!
        {
            try
            {
                StringHelper.OnDebug(eDebugEventType.Info, "UResetPinText");
                Thread.Sleep(1000);
                keypadText = "";
                var ui = o as BasicTriList;
                if (ui != null)
                    SetSerialJoin(ui, SER_INPUT, keypadText);
            }
            catch (Exception e)
            {
                StringHelper.OnDebug(eDebugEventType.Info, "ResetPinText exception: {0}", e.Message);
            }
            return null;
        }

        void SmartObject_DPad_SigChange(BasicTriListWithSmartObject currentDevice, SmartObjectEventArgs args) 
        { 
            if (args.Sig.BoolValue)
            {
                switch (args.Sig.Name.ToUpper())
                {
                    case "UP"    : StringHelper.OnDebug(eDebugEventType.Info, "Up pressed"    ); break; // up
                    case "DOWN"  : StringHelper.OnDebug(eDebugEventType.Info, "Down pressed"  ); break; // dn
                    case "LEFT"  : StringHelper.OnDebug(eDebugEventType.Info, "Left pressed"  ); break; // le
                    case "RIGHT" : StringHelper.OnDebug(eDebugEventType.Info, "Right pressed" ); break; // ri
                    case "CENTER": StringHelper.OnDebug(eDebugEventType.Info, "Center pressed"); break; // OK
                    default: 
                        StringHelper.OnDebug(eDebugEventType.Info, "Unhandled keypad button {0} pressed, name:{1}", args.Sig.Number, args.Sig.Name); 
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
                        StringHelper.OnDebug(eDebugEventType.Info, "Press event");
                        switch (sig.Number)
                        {
                            //case 1: break;
                            default:
                                // toggle the button feedback and put some text onto it
                                SG.ToggleSmartObjectSelected(so, (int)sig.Number); // standard button lists don't support feedback  so this doesn't do anything
                                string buttonText = "Item " + sig.Number.ToString();
                                SG.SetSmartObjectText       (so, (int)sig.Number, buttonText);

                                // soDynBtnList uses dynamic IconAnalogs, of type MediaTransports
                                SG.ToggleSmartObjectVisible(soDynBtnList, (int)sig.Number);       // toggle visibility
                                SG.SetSmartObjectEnabled   (soDynBtnList, (int)sig.Number, true); // enable
                                SG.SetSmartObjectIconAnalog(soDynBtnList, (int)sig.Number, (ushort)sig.Number); // set icon to the next analog

                                // soDynIconList uses dynamic IconSerials, of type IconsLg
                                SG.ToggleSmartObjectVisible(soDynIconList, (int)sig.Number);       // toggle visibility
                                SG.SetSmartObjectEnabled   (soDynIconList, (int)sig.Number, true); // enable
                                SG.SetSmartObjectIconSerial(soDynIconList, (int)sig.Number, UI.IconsLgDict[(ushort)sig.Number]); // set icon to the next serial
                                break;
                        }
                    }
                    else
                    {
                        StringHelper.OnDebug(eDebugEventType.Info, "Release event");
                    }
                    break;
                case eSigType.UShort:
                    StringHelper.OnDebug(eDebugEventType.Info, "UShortValue: {0}", sig.UShortValue.ToString());
                    break;
                case eSigType.String:
                    StringHelper.OnDebug(eDebugEventType.Info, "StringValue: {0}", sig.StringValue);
                    break;
                default:
                    StringHelper.OnDebug(eDebugEventType.Info, "Unhandled sig type: {0}", sig.Type.ToString());
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
                        StringHelper.OnDebug(eDebugEventType.Info, "Press event");
                        switch (sig.Number)
                        {
                            default:
                                int number = StringHelper.Atoi(sig.Name); // Number is offset by 10 so we need item with no offset
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
                        StringHelper.OnDebug(eDebugEventType.Info, "Release event");
                    }
                    break;
                case eSigType.UShort:
                    StringHelper.OnDebug(eDebugEventType.Info, "UShortValue: {0}", sig.UShortValue.ToString());
                    break;
                case eSigType.String:
                    StringHelper.OnDebug(eDebugEventType.Info, "StringValue: {0}", sig.StringValue);
                    break;
                default:
                    StringHelper.OnDebug(eDebugEventType.Info, "Unhandled sig type: {0}", sig.Type.ToString());
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
                        StringHelper.OnDebug(eDebugEventType.Info, "Press event");
                        switch (sig.Number)
                        {
                            default:
                                int number = StringHelper.Atoi(sig.Name); // Number is offset by 10 so we need item with no offset
                                // toggle the button feedback and put some text onto it
                                SG.ToggleSmartObjectDigitalJoin(so, (int)sig.Number);
                                string buttonText = "Item " + sig.Number.ToString() + " " + SG.GetSmartObjectDigitalJoin(so, (int)sig.Number).ToString();
                                //SG.SetSmartObjectText(so, (int)sig.Number, buttonText);
                                SG.SetSmartObjectText(so, number, buttonText);

                                SG.ToggleSmartObjectEnabled(soDynBtnList, number);       // enable

                                SG.ToggleSmartObjectDigitalJoin(soBtnList, number);
                                SG.SetSmartObjectText          (soBtnList, number, buttonText);
                                break;
                        }
                    }
                    else
                    {
                        StringHelper.OnDebug(eDebugEventType.Info, "Release event");
                    }
                    break;
                case eSigType.UShort:
                    StringHelper.OnDebug(eDebugEventType.Info, "UShortValue: {0}", sig.UShortValue.ToString());
                    break;
                case eSigType.String:
                    StringHelper.OnDebug(eDebugEventType.Info, "StringValue: {0}", sig.StringValue);
                    break;
                default:
                    StringHelper.OnDebug(eDebugEventType.Info, "Unhandled sig type: {0}", sig.Type.ToString());
                    break;
            }
        }

        #region join methods

        void SetDigitalJoin(BasicTriList currentDevice, uint number, bool value)
        {
            currentDevice.BooleanInput[number].BoolValue = value;
        }
        void ToggleDigitalJoin(BasicTriList currentDevice, uint number)
        {
            currentDevice.BooleanInput[number].BoolValue = !currentDevice.BooleanInput[number].BoolValue;
        }
        void PulseDigitalJoin(BasicTriList currentDevice, uint number)
        {
            currentDevice.BooleanInput[number].Pulse();
        }
        void SetAnalogJoin(BasicTriList currentDevice, uint number, ushort value)
        {
            currentDevice.UShortInput[number].UShortValue = value;
        }
        void SetSerialJoin(BasicTriList currentDevice, uint number, string value)
        {
            currentDevice.StringInput[number].StringValue = value;
        }

        #endregion

    }
}