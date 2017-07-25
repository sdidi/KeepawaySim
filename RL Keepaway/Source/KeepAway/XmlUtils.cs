using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
namespace Keepaway
{
    class XmlUtils
    {


        /// <summary>
        /// Parse the inner text of element with the given name as an integer. If element is missing or parsing fails then
        /// throws an ArgumentException.
        /// </summary>
        public static int GetValueAsInt(XmlElement xmlParent, string elemName)
        {
            int? val = TryGetValueAsInt(xmlParent, elemName);
            if (null == val)
            {
                throw new ArgumentException(string.Format("Missing [{0}] configuration setting.", elemName));
            }
            return val.Value;
        }

        /// <summary>
        /// Parse the inner text of element with the given name as an integer. If element is missing or parsing fails then
        /// returns null.
        /// </summary>
        public static int? TryGetValueAsInt(XmlElement xmlParent, string elemName)
        {
            XmlElement xmlElem = xmlParent.SelectSingleNode(elemName) as XmlElement;
            if (null == xmlElem)
            {
                return null;
            }

            string valStr = xmlElem.InnerText;
            if (string.IsNullOrEmpty(valStr))
            {
                return null;
            }

            int result;
            if (int.TryParse(valStr, out result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// Parse the inner text of element with the given name as a double. If element is missing or parsing fails then
        /// throws an ArgumentException.
        /// </summary>
        public static double GetValueAsDouble(XmlElement xmlParent, string elemName)
        {
            double? val = TryGetValueAsDouble(xmlParent, elemName);
            if (null == val)
            {
                throw new ArgumentException(string.Format("Missing [{0}] configuration setting.", elemName));
            }
            return val.Value;
        }

        /// <summary>
        /// Parse the inner text of element with the given name as a double. If element is missing or parsing fails then
        /// returns null.
        /// </summary>
        public static double? TryGetValueAsDouble(XmlElement xmlParent, string elemName)
        {
            XmlElement xmlElem = xmlParent.SelectSingleNode(elemName) as XmlElement;
            if (null == xmlElem)
            {
                return null;
            }

            string valStr = xmlElem.InnerText;
            if (string.IsNullOrEmpty(valStr))
            {
                return null;
            }

            double result;
            if (double.TryParse(valStr, out result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// Parse the inner text of element with the given name as a boolean. If element is missing or parsing fails then
        /// throws an ArgumentException.
        /// </summary>
        public static bool GetValueAsBool(XmlElement xmlParent, string elemName)
        {
            bool? val = TryGetValueAsBool(xmlParent, elemName);
            if (null == val)
            {
                throw new ArgumentException(string.Format("Missing [{0}] configuration setting.", elemName));
            }
            return val.Value;
        }

        /// <summary>
        /// Parse the inner text of element with the given name as a boolean. If element is missing or parsing fails then
        /// returns null.
        /// </summary>
        public static bool? TryGetValueAsBool(XmlElement xmlParent, string elemName)
        {
            XmlElement xmlElem = xmlParent.SelectSingleNode(elemName) as XmlElement;
            if (null == xmlElem)
            {
                return null;
            }

            string valStr = xmlElem.InnerText;
            if (string.IsNullOrEmpty(valStr))
            {
                return null;
            }

            bool result;
            if (bool.TryParse(valStr, out result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// Read the inner text of element with the given name. If element is missing then throws an ArgumentException.
        /// </summary>
        public static string GetValueAsString(XmlElement xmlParent, string elemName)
        {
            string val = TryGetValueAsString(xmlParent, elemName);
            if (null == val)
            {
                throw new ArgumentException(string.Format("Missing [{0}] configuration setting.", elemName));
            }
            return val;
        }

        /// <summary>
        /// Read the inner text of element with the given name. If element is missing then returns null.
        /// </summary>
        public static string TryGetValueAsString(XmlElement xmlParent, string elemName)
        {
            XmlElement xmlElem = xmlParent.SelectSingleNode(elemName) as XmlElement;
            if (null == xmlElem)
            {
                return null;
            }

            string valStr = xmlElem.InnerText;
            if (string.IsNullOrEmpty(valStr))
            {
                return null;
            }
            return valStr;
        }
    }
}
