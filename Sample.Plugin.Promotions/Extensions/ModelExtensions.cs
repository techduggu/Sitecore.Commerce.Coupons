using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Plugin.Rules;
using Sitecore.Framework.Rules;
using Sitecore.Framework.Rules.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sample.Plugin.Promotions.Extensions
{
    public static class ModelExtensions
    {
        public static ICondition ExtendedConvertToCondition(this ConditionModel model, IEntityMetadata metaData, IEntityRegistry registry, IServiceProvider services)
        {
            if (((IEnumerable<object>)metaData.Type.GetCustomAttributes(typeof(ObsoleteAttribute), false)).Any<object>())
                return (ICondition)null;
            ICondition instance1 = ActivatorUtilities.CreateInstance(services, metaData.Type, Array.Empty<object>()) as ICondition;
            if (instance1 == null)
                return (ICondition)null;
            PropertyInfo[] properties = instance1.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            if (((IEnumerable<PropertyInfo>)properties).Any<PropertyInfo>((Func<PropertyInfo, bool>)(p => ModelExtensions.IsBinaryOperator(p.PropertyType))))
            {
                PropertyInfo propertyInfo = ((IEnumerable<PropertyInfo>)properties).FirstOrDefault<PropertyInfo>((Func<PropertyInfo, bool>)(p => ModelExtensions.IsBinaryOperator(p.PropertyType)));
                PropertyModel operatorModelProperty = model.Properties.FirstOrDefault<PropertyModel>((Func<PropertyModel, bool>)(x => x.IsOperator));
                if (operatorModelProperty != null)
                {
                    IEntityMetadata entityMetadata = registry.GetOperators().FirstOrDefault<IEntityMetadata>((Func<IEntityMetadata, bool>)(m => m.Type.FullName.Equals(operatorModelProperty.Value, StringComparison.OrdinalIgnoreCase)));
                    object instance2 = ActivatorUtilities.CreateInstance(services, entityMetadata?.Type, Array.Empty<object>());
                    if ((object)propertyInfo != null)
                        propertyInfo.SetValue((object)instance1, instance2);
                }
            }
            foreach (PropertyModel property1 in (IEnumerable<PropertyModel>)model.Properties)
            {
                if (!property1.IsOperator)
                {
                    PropertyInfo property2 = instance1.GetType().GetProperty(property1.Name, BindingFlags.Instance | BindingFlags.Public);
                    Type type = (!property2.PropertyType.IsGenericType ? 0 : (typeof(IRuleValue<>).IsAssignableFrom(property2.PropertyType.GetGenericTypeDefinition()) ? 1 : 0)) != 0 ? ((IEnumerable<Type>)property2.PropertyType.GetGenericArguments()).FirstOrDefault<Type>() : property2.PropertyType;
                    if (!(type == (Type)null))
                    {
                        string fullName = type.FullName;
                        if (!(fullName == "System.DateTime"))
                        {
                            if (!(fullName == "System.DateTimeOffSet"))
                            {
                                if (!(fullName == "System.Int32"))
                                {
                                    if (!(fullName == "System.Boolean")) //our extended boolean condition
                                    {
                                        if (fullName == "System.Decimal")
                                        {
                                            Decimal result;
                                            Decimal.TryParse(property1.Value, out result);
                                            LiteralRuleValue<Decimal> literalRuleValue = new LiteralRuleValue<Decimal>()
                                            {
                                                Value = result
                                            };
                                            property2.SetValue((object)instance1, (object)literalRuleValue, (object[])null);
                                        }
                                        else
                                        {
                                            LiteralRuleValue<string> literalRuleValue = new LiteralRuleValue<string>()
                                            {
                                                Value = property1.Value
                                            };
                                            property2.SetValue((object)instance1, (object)literalRuleValue, (object[])null);
                                        }
                                    }
                                    else
                                    {
                                        bool result;
                                        bool.TryParse(property1.Value, out result);
                                        LiteralRuleValue<bool> literalRuleValue = new LiteralRuleValue<bool>()
                                        {
                                            Value = result
                                        };
                                        property2.SetValue((object)instance1, (object)literalRuleValue, (object[])null);
                                    }
                                }
                                else
                                {
                                    int result;
                                    int.TryParse(property1.Value, out result);
                                    LiteralRuleValue<int> literalRuleValue = new LiteralRuleValue<int>()
                                    {
                                        Value = result
                                    };
                                    property2.SetValue((object)instance1, (object)literalRuleValue, (object[])null);
                                }
                            }
                            else
                            {
                                DateTimeOffset result;
                                DateTimeOffset.TryParse(property1.Value, out result);
                                LiteralRuleValue<DateTimeOffset> literalRuleValue = new LiteralRuleValue<DateTimeOffset>()
                                {
                                    Value = result
                                };
                                property2.SetValue((object)instance1, (object)literalRuleValue, (object[])null);
                            }
                        }
                        else
                        {
                            DateTime result;
                            DateTime.TryParse(property1.Value, out result);
                            LiteralRuleValue<DateTime> literalRuleValue = new LiteralRuleValue<DateTime>()
                            {
                                Value = result
                            };
                            property2.SetValue((object)instance1, (object)literalRuleValue, (object[])null);
                        }
                    }
                }
            }
            return instance1;
        }

        public static bool IsValid(this ConditionModel model)
        {
            if (model.Properties != null && !model.Properties.Any<PropertyModel>((Func<PropertyModel, bool>)(p => string.IsNullOrEmpty(p.Value))))
                return model.ConditionOperator != null;
            return false;
        }

        public static bool SetPropertyValue(this ConditionModel model, string propertyName, string propertyValue)
        {
            if (string.IsNullOrEmpty(propertyValue) || string.IsNullOrEmpty(propertyName) || (model.Properties == null || !model.Properties.Any<PropertyModel>()))
                return true;
            PropertyModel propertyModel = model.Properties.FirstOrDefault<PropertyModel>((Func<PropertyModel, bool>)(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)));
            if (propertyModel == null)
                return true;
            if (propertyName.Equals("TargetItemId", StringComparison.OrdinalIgnoreCase))
            {
                string[] strArray = propertyValue.Split('|');
                if (strArray.Length == 2)
                    propertyValue = string.Format("{0}|", (object)propertyValue);
                else if (strArray.Length > 3 || strArray.Length <= 1)
                    return true;
            }
            bool flag = false;
            string displayType = propertyModel.DisplayType;
            if (!(displayType == "System.DateTime"))
            {
                if (!(displayType == "System.DateTimeOffset"))
                {
                    if (!(displayType == "System.Int32"))
                    {
                        if (!(displayType == "System.Boolean")) //our extended boolean condition
                        {
                            if (displayType == "System.Decimal")
                            {
                                Decimal result;
                                flag = !Decimal.TryParse(propertyValue, out result);
                            }
                        }
                        else
                        {
                            bool result;
                            flag = !bool.TryParse(propertyValue, out result);
                        }
                    }
                    else
                    {
                        int result;
                        flag = !int.TryParse(propertyValue, out result);
                    }
                }
                else
                {
                    DateTimeOffset result;
                    flag = !DateTimeOffset.TryParse(propertyValue, out result);
                }
            }
            else
            {
                DateTime result;
                flag = !DateTime.TryParse(propertyValue, out result);
            }
            if (flag)
                return true;
            propertyModel.Value = propertyValue;
            return false;
        }

        public static IAction ExtendedConvertToAction(this ActionModel model, IEntityMetadata metaData, IEntityRegistry registry, IServiceProvider services)
        {
            if (((IEnumerable<object>)metaData.Type.GetCustomAttributes(typeof(ObsoleteAttribute), false)).Any<object>())
                return (IAction)null;
            IAction instance1 = ActivatorUtilities.CreateInstance(services, metaData.Type, Array.Empty<object>()) as IAction;
            if (instance1 == null)
                return (IAction)null;
            PropertyInfo[] properties = instance1.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            if (((IEnumerable<PropertyInfo>)properties).Any<PropertyInfo>((Func<PropertyInfo, bool>)(p => ModelExtensions.IsBinaryOperator(p.PropertyType))))
            {
                PropertyInfo propertyInfo = ((IEnumerable<PropertyInfo>)properties).FirstOrDefault<PropertyInfo>((Func<PropertyInfo, bool>)(p => ModelExtensions.IsBinaryOperator(p.PropertyType)));
                PropertyModel operatorModelProperty = model.Properties.FirstOrDefault<PropertyModel>((Func<PropertyModel, bool>)(x => x.IsOperator));
                if (operatorModelProperty != null)
                {
                    IEntityMetadata entityMetadata = registry.GetOperators().FirstOrDefault<IEntityMetadata>((Func<IEntityMetadata, bool>)(m => m.Type.FullName.Equals(operatorModelProperty.Value, StringComparison.OrdinalIgnoreCase)));
                    object instance2 = ActivatorUtilities.CreateInstance(services, entityMetadata?.Type, Array.Empty<object>());
                    if ((object)propertyInfo != null)
                        propertyInfo.SetValue((object)instance1, instance2);
                }
            }
            foreach (PropertyModel property1 in (IEnumerable<PropertyModel>)model.Properties)
            {
                if (!property1.IsOperator)
                {
                    PropertyInfo property2 = instance1.GetType().GetProperty(property1.Name, BindingFlags.Instance | BindingFlags.Public);
                    Type type = (!property2.PropertyType.IsGenericType ? 0 : (typeof(IRuleValue<>).IsAssignableFrom(property2.PropertyType.GetGenericTypeDefinition()) ? 1 : 0)) != 0 ? ((IEnumerable<Type>)property2.PropertyType.GetGenericArguments()).FirstOrDefault<Type>() : property2.PropertyType;
                    if (!(type == (Type)null))
                    {
                        string fullName = type.FullName;
                        if (!(fullName == "System.DateTime"))
                        {
                            if (!(fullName == "System.DateTimeOffset"))
                            {
                                if (!(fullName == "System.Int32"))
                                {
                                    if (!(fullName == "System.Boolean")) //our extended boolean condition
                                    {
                                        if (fullName == "System.Decimal")
                                        {
                                            Decimal result;
                                            Decimal.TryParse(property1.Value, out result);
                                            LiteralRuleValue<Decimal> literalRuleValue = new LiteralRuleValue<Decimal>()
                                            {
                                                Value = result
                                            };
                                            property2.SetValue((object)instance1, (object)literalRuleValue, (object[])null);
                                        }
                                        else
                                        {
                                            LiteralRuleValue<string> literalRuleValue = new LiteralRuleValue<string>()
                                            {
                                                Value = property1.Value
                                            };
                                            property2.SetValue((object)instance1, (object)literalRuleValue, (object[])null);
                                        }
                                    }
                                    else
                                    {
                                        bool result;
                                        bool.TryParse(property1.Value, out result);
                                        LiteralRuleValue<bool> literalRuleValue = new LiteralRuleValue<bool>()
                                        {
                                            Value = result
                                        };
                                        property2.SetValue((object)instance1, (object)literalRuleValue, (object[])null);
                                    }
                                }
                                else
                                {
                                    int result;
                                    int.TryParse(property1.Value, out result);
                                    LiteralRuleValue<int> literalRuleValue = new LiteralRuleValue<int>()
                                    {
                                        Value = result
                                    };
                                    property2.SetValue((object)instance1, (object)literalRuleValue, (object[])null);
                                }
                            }
                            else
                            {
                                DateTimeOffset result;
                                DateTimeOffset.TryParse(property1.Value, out result);
                                LiteralRuleValue<DateTimeOffset> literalRuleValue = new LiteralRuleValue<DateTimeOffset>()
                                {
                                    Value = result
                                };
                                property2.SetValue((object)instance1, (object)literalRuleValue, (object[])null);
                            }
                        }
                        else
                        {
                            DateTime result;
                            DateTime.TryParse(property1.Value, out result);
                            LiteralRuleValue<DateTime> literalRuleValue = new LiteralRuleValue<DateTime>()
                            {
                                Value = result
                            };
                            property2.SetValue((object)instance1, (object)literalRuleValue, (object[])null);
                        }
                    }
                }
            }
            return instance1;
        }

        public static bool IsValid(this ActionModel model)
        {
            if (model.Properties != null)
                return !model.Properties.Any<PropertyModel>((Func<PropertyModel, bool>)(p => string.IsNullOrEmpty(p.Value)));
            return false;
        }

        public static bool SetPropertyValue(this ActionModel model, string propertyName, string propertyValue)
        {
            if (string.IsNullOrEmpty(propertyValue) || string.IsNullOrEmpty(propertyName) || (model.Properties == null || !model.Properties.Any<PropertyModel>()))
                return true;
            PropertyModel propertyModel = model.Properties.FirstOrDefault<PropertyModel>((Func<PropertyModel, bool>)(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)));
            if (propertyModel == null)
                return true;
            bool flag = false;
            string displayType = propertyModel.DisplayType;
            if (!(displayType == "System.DateTime"))
            {
                if (!(displayType == "System.DateTimeOffset"))
                {
                    if (!(displayType == "System.Int32"))
                    {
                        if (!(displayType == "System.Boolean")) //our extended boolean condition
                        {
                            if (displayType == "System.Decimal")
                            {
                                Decimal result;
                                flag = !Decimal.TryParse(propertyValue, out result);
                            }
                        }
                        else
                        {
                            bool result;
                            flag = !bool.TryParse(propertyValue, out result);
                        }
                    }
                    else
                    {
                        int result;
                        flag = !int.TryParse(propertyValue, out result);
                    }
                }
                else
                {
                    DateTimeOffset result;
                    flag = !DateTimeOffset.TryParse(propertyValue, out result);
                }
            }
            else
            {
                DateTime result;
                flag = !DateTime.TryParse(propertyValue, out result);
            }
            if (flag)
                return true;
            propertyModel.Value = propertyValue;
            return false;
        }

        private static bool IsBinaryOperator(Type type)
        {
            if (type.IsGenericType)
                return type.GetGenericTypeDefinition() == typeof(IBinaryOperator<,>);
            return false;
        }
    }
}
