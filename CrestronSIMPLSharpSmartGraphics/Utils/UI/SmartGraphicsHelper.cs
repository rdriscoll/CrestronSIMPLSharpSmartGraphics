using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace AVPlus.Utils.UI
{
    public static class SmartGraphicsHelper
    {
        // Crestrons implementation of Smart joins is terrible, hopefully this library helps.
        // use inputs (e.g. BooleanInput as opposed to BooleanOutput) for setting values.

        #region digitals

        // Standard lists BooleanInput["Item 1 Pressed" ]
        // Dynamic  lists BooleanInput["Item 1 Selected"]
        public static void SetSmartObjectDigitalJoin(SmartObject so, string name, bool state)
        {
            try
            {
                so.BooleanInput[name].BoolValue = state;
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("SetSmartObjectDigitalJoin exception: {0}", e.Message);
            }
        }

        public static void SetSmartObjectSelected   (SmartObject so, int index, bool state)
        {
            var name = "Item " + index.ToString() + " Selected";
            if (so.StringInput.Contains(name))
                SetSmartObjectDigitalJoin(so, name, state);
            else
            {
                name = "Item " + index.ToString() + " Pressed";
                if (so.StringInput.Contains(name))
                    SetSmartObjectDigitalJoin(so, name, state);
                else
                    so.BooleanInput[(ushort)index].BoolValue = state;
            }
        }
        public static void SetSmartObjectSelected   (BasicTriListWithSmartObject device, uint SmartObjectID, int index, bool state)
        {
            SetSmartObjectSelected(device.SmartObjects[SmartObjectID], index, state);
        }
        public static void ToggleSmartObjectSelected(SmartObject so, int index)
        {
            try
            {
                var name = "Item " + index.ToString() + " Selected";
                if (so.StringInput.Contains(name))
                    so.BooleanInput[name].BoolValue = !so.BooleanInput[name].BoolValue;
                else
                {
                    name = "Item " + index.ToString() + " Pressed";
                    if (so.StringInput.Contains(name))
                        so.BooleanInput[name].BoolValue = !so.BooleanInput[name].BoolValue;
                    else
                        so.BooleanInput[(ushort)index].BoolValue = !so.BooleanInput[(ushort)index].BoolValue;
                }
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("ToggleSmartObjectSelected exception: {0}", e.Message);
            }
        }
        public static void ToggleSmartObjectSelected(BasicTriListWithSmartObject device, uint SmartObjectID, int index)
        {
            ToggleSmartObjectSelected(device.SmartObjects[SmartObjectID], index);
        }

        public static void SetSmartObjectVisible    (SmartObject so, int index, bool state)
        {
            var name = "Item " + index.ToString() + " Visible";
            SetSmartObjectDigitalJoin(so, name, state);
        }
        public static void SetSmartObjectVisible    (BasicTriListWithSmartObject device, uint SmartObjectID, int index, bool state)
        {
            SetSmartObjectVisible(device.SmartObjects[SmartObjectID], index, state);
        }
        public static void ToggleSmartObjectVisible (SmartObject so, int index)
        {
            try
            {
                var name = "Item " + index.ToString() + " Visible";
                so.BooleanInput[name].BoolValue = !so.BooleanInput[name].BoolValue;
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("ToggleSmartObjectVisible exception: {0}", e.Message);
            }
        }
        public static void ToggleSmartObjectVisible (BasicTriListWithSmartObject device, uint SmartObjectID, int index)
        {
            ToggleSmartObjectVisible(device.SmartObjects[SmartObjectID], index);
        }

        public static void SetSmartObjectEnabled    (SmartObject so, int index, bool state)
        {
            var name = "Item " + index.ToString() + " Enabled";
            SetSmartObjectDigitalJoin(so, name, state);
        }
        public static void SetSmartObjectEnabled    (BasicTriListWithSmartObject device, uint SmartObjectID, int index, bool state)
        {
            SetSmartObjectEnabled(device.SmartObjects[SmartObjectID], index, state);
        }
        public static void ToggleSmartObjectEnabled (SmartObject so, int index)
        {
            try
            {
                var name = "Item " + index.ToString() + " Enabled";
                so.BooleanInput[name].BoolValue = !so.BooleanInput[name].BoolValue;
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("ToggleSmartObjectEnable exception: {0}", e.Message);
            }
        }
        public static void ToggleSmartObjectEnabled (BasicTriListWithSmartObject device, uint SmartObjectID, int index)
        {
            ToggleSmartObjectEnabled(device.SmartObjects[SmartObjectID], index);
        }

        public static bool GetSmartObjectDigitalJoin(SmartObject so, string name)
        {
            try
            {
                return so.BooleanInput[name].BoolValue;
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("GetSmartObjectDigitalJoin exception: {0}", e.Message);
            }
            return false;
        }
        public static bool GetSmartObjectDigitalJoin(BasicTriListWithSmartObject device, uint SmartObjectID, string name)
        {
            return GetSmartObjectDigitalJoin(device.SmartObjects[SmartObjectID], name);
        }
        public static bool GetSmartObjectDigitalJoin(SmartObject so, int index)
        {
            var name = "Item " + index.ToString() + " Selected";
            if (so.StringInput.Contains(name))
                return so.BooleanInput[name].BoolValue;
            else
            {
                name = "Item " + index.ToString() + " Pressed";
                if (so.StringInput.Contains(name))
                    return so.BooleanInput[name].BoolValue;
                else
                    return so.BooleanInput[(ushort)index].BoolValue;
            }
        
        }
        public static bool GetSmartObjectDigitalJoin(BasicTriListWithSmartObject device, uint SmartObjectID, int index)
        {
            return GetSmartObjectDigitalJoin(device.SmartObjects[SmartObjectID], index);
        }

        // SmartObject join number doesn't necessarily start at 1 so use the "selected" functions unless you are sure you have the right join
        public static void SetSmartObjectDigitalJoin   (SmartObject so, int index, bool state)
        {
            try
            {
                so.BooleanInput[(ushort)index].BoolValue = state;
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("SetSmartObjectDigitalJoin exception: {0}", e.Message);
            }
        }
        public static void SetSmartObjectDigitalJoin   (BasicTriListWithSmartObject device, uint SmartObjectID, int index, bool state)
        {
            SetSmartObjectDigitalJoin(device.SmartObjects[SmartObjectID], index, state);
        }
        public static void ToggleSmartObjectDigitalJoin(SmartObject so, int index)
        {
            try
            {
                so.BooleanInput[(ushort)index].BoolValue = !so.BooleanInput[(ushort)index].BoolValue;
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("ToggleSmartObjectDigitalJoin exception: {0}", e.Message);
            }
        }
        public static void ToggleSmartObjectDigitalJoin(BasicTriListWithSmartObject device, uint SmartObjectID, int index)
        {
            ToggleSmartObjectDigitalJoin(device.SmartObjects[SmartObjectID], index);
        }

        #endregion

        #region analogs

        // most analogs use format "Set Item 1 Text"

        public static void SetSmartObjectValue(SmartObject so, string name, ushort state)
        {
            try
            {
                so.UShortInput[name].UShortValue = state;
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("SetSmartObjectValue exception: {0}", e.Message);
            }
        }
        public static void SetSmartObjectValue(BasicTriListWithSmartObject device, uint SmartObjectID, string name, ushort state)
        {
            SetSmartObjectValue(device.SmartObjects[SmartObjectID], name, state);
        }
        public static void SetSmartObjectValue(SmartObject so, int index, ushort state)
        {
            try
            {
                so.UShortInput[(ushort)index].UShortValue = state;
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("SetSmartObjectDigitalJoin exception: {0}", e.Message);
            }
        }
        public static void SetSmartObjectValue(BasicTriListWithSmartObject device, uint SmartObjectID, int index, ushort state)
        {
            SetSmartObjectValue(device.SmartObjects[SmartObjectID], index, state);
        }
        // Icons "Set Item 1 Analog"
        public static void SetSmartObjectIconAnalog(SmartObject so, int index, ushort state)
        {
            var name = "Set Item " + index.ToString() + " Icon Analog";
            SetSmartObjectValue(so, name, state);
        }

        #endregion

        #region serials

        public static void SetSmartObjectText(SmartObject so, string name, string state)
        {
            try
            {
                if (so.StringInput.Contains(name))
                    so.StringInput[name].StringValue = state;
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("SetSmartObjectText exception: {0}", e.Message);
            }
        }
        public static void SetSmartObjectText(SmartObject so, int index, string state)
        {
            // Standard lists use serial format "Set Item 1 Text"
            // Dynamic lists use serial format "Item 1 Text"
            var name = "Item " + index.ToString() + " Text";
            if (!so.StringInput.Contains(name))
                name = "Set " + name;
            SetSmartObjectText(so, name, state);
        }
        public static void SetSmartObjectText(BasicTriListWithSmartObject device, uint SmartObjectID, int index, string state)
        {
            SetSmartObjectText(device.SmartObjects[SmartObjectID], index, state);
        }
        // some serials use format "text-i1"
        public static void SetSmartObjectInputText(SmartObject so, int index, string state)
        {
            var name = "text-i" + index.ToString();
            SetSmartObjectText(so, name, state);
        }
        // icon text matches 
        public static void SetSmartObjectIconSerial(SmartObject so, int index, string iconName)
        {
            var name = "Set Item " + index.ToString() + " Icon Serial";
            SetSmartObjectText(so, name, iconName);
        }

        #endregion

        public static void PrintSmartObjectSigNames(SmartObject so)
        {
            CrestronConsole.PrintLine("SmartObject Object ID {0}, on {1}", so.ID, so.Device.ToString());
            foreach (Sig sig in so.BooleanInput)
                CrestronConsole.PrintLine("BooleanInput Signal name: {0}", sig.Name);
            foreach (Sig sig in so.BooleanOutput)
                CrestronConsole.PrintLine("BooleanOutput Signal name: {0}", sig.Name);
            foreach (Sig sig in so.StringInput)
                CrestronConsole.PrintLine("StringInput Signal name: {0}", sig.Name);
            foreach (Sig sig in so.StringOutput)
                CrestronConsole.PrintLine("StringOutput Signal name: {0}", sig.Name);
            foreach (Sig sig in so.UShortInput)
                CrestronConsole.PrintLine("UShortInput Signal name: {0}", sig.Name);
            foreach (Sig sig in so.UShortOutput)
                CrestronConsole.PrintLine("UShortOutput Signal name: {0}", sig.Name);
        }
    }
}