using Naveego.Sdk.Plugins;
using PluginZohoCreator.DataContracts;


namespace PluginZohoCreator.API.Discover
{
    public static partial class Discover
    {
        /// <summary>
        /// Gets the Naveego type from the provided Zoho information
        /// </summary>
        /// <param name="field"></param>
        /// <returns>The property type</returns>
        private static PropertyType GetPropertyType(Field field)
        {
            return PropertyType.String;
//            switch (field.ApiType)
//            {
//                case 6:
//                case 7:
//                    return PropertyType.Float;
//                case 5:
//                case 9:
//                    return PropertyType.Integer;
//                case 10:
//                case 11:
//                    return PropertyType.Datetime;
//                default:
//                    if (field.MaxChar > 1024)
//                    {
//                        return PropertyType.Text;
//                    }
//                    else
//                    {
//                        return PropertyType.String;
//                    }
//            }
        }
        
        /// <summary>
        /// Gets the Naveego type from the provided Zoho information
        /// </summary>
        /// <param name="field"></param>
        /// <returns>The property type</returns>
        private static PropertyType GetPropertyType(CustomField field)
        {
            return PropertyType.String;
//            switch (field.Type)
//            {
//                case 6:
//                case 7:
//                    return PropertyType.Float;
//                case 5:
//                case 9:
//                    return PropertyType.Integer;
//                case 10:
//                case 11:
//                    return PropertyType.Datetime;
//                default:
//                    if (field.MaxChar > 1024)
//                    {
//                        return PropertyType.Text;
//                    }
//                    else
//                    {
//                        return PropertyType.String;
//                    }
//            }
        }
    }
}