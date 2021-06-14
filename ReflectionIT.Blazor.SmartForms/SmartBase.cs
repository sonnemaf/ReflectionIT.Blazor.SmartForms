using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace ReflectionIT.Blazor.SmartForms {

    public abstract class SmartBase<TItem> : InputBase<TItem> {

        [Parameter] public int? LabelColumnSizeMedium { get; set; }
        [Parameter] public int? LabelColumSizeLarge { get; set; }

        [Parameter] public string CssClassLabel { get; set; } = "form-control-label";
        [Parameter] public string CssClassInput { get; set; } = "form-control";
        [Parameter] public string CssClassValidation { get; set; } = "form-control-validation";
        [Parameter] public string CssClassRequired { get; set; }

        [Parameter] public bool DisplayLabel { get; set; } = true;
        [Parameter] public bool DisplayValidationMessage { get; set; } = true;

        [Parameter] public string Id { get; set; }
        [Parameter] public Expression<Func<TItem>> For { get; set; }
        [Parameter] public string Prefix { get; set; }
        [Parameter] public string Suffix { get; set; }

        protected override bool TryParseValueFromString(string value, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TItem result, [System.Diagnostics.CodeAnalysis.NotNullWhen(false)] out string validationErrorMessage) {
            // Let's Blazor convert the value for us
            if (BindConverter.TryConvertTo(value, System.Globalization.CultureInfo.CurrentCulture, out TItem parsedValue)) {
                result = parsedValue;
                validationErrorMessage = null;
                return true;
            }

            result = default;
            validationErrorMessage = $"The {FieldIdentifier.FieldName} field is not valid.";
            return false;
        }

        protected string GetDisplayName() {
            var expression = (MemberExpression)For.Body;
            var value = expression.Member.GetCustomAttribute<DisplayAttribute>();
            return value?.Name ?? expression.Member.Name ?? string.Empty;
        }

        protected string GetId() {
            var expression = (MemberExpression)For.Body;
            return expression.Member.Name ?? string.Empty;
        }
        protected bool IsRow => this.LabelColumnSizeMedium.HasValue || this.LabelColumSizeLarge.HasValue;

        /// <summary>
        /// Determines if a type is numeric.  Nullable numeric types are considered numeric.
        /// </summary>
        /// <remarks>
        /// Boolean is not considered numeric.
        /// </remarks>
        protected static bool IsNumericType(Type type) {
            if (type == null) {
                return false;
            }
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                case TypeCode.Object:
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                        return IsNumericType(Nullable.GetUnderlyingType(type));
                    }
                    return false;
            }
            return false;
        }
        
    }
}
