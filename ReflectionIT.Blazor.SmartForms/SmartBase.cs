using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

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

        protected override void OnParametersSet() {
            var dict = this.AdditionalAttributes is null ? new Dictionary<string, object>() : new Dictionary<string, object>(this.AdditionalAttributes, StringComparer.OrdinalIgnoreCase);
            base.OnParametersSet();

            var type = typeof(TItem);
            if (IsNumericType(type)) {
                dict["type"] = "number";
            }
            if (type == typeof(DateTime) || type == typeof(DateTime?)) {
                dict["type"] = "date";
            }

            var expression = (MemberExpression)For.Body;
            var range = expression.Member.GetCustomAttribute<RangeAttribute>();
            if (range is not null) {
                if (range.Minimum is not null && !dict.ContainsKey("min")) {
                    dict["min"] = range.Minimum;
                }
                if (range.Maximum is not null && !dict.ContainsKey("max")) {
                    dict["max"] = range.Maximum;
                }
            }

            var stringLength = expression.Member.GetCustomAttribute<StringLengthAttribute>();
            if (stringLength is not null && !dict.ContainsKey("maxlength")) {
                dict["maxlength"] = stringLength.MaximumLength;
            }

            var RequiredAttribute = expression.Member.GetCustomAttribute<RequiredAttribute>();
            if (RequiredAttribute is not null) {
                if (!dict.ContainsKey("required")) {
                    dict["required"] = string.Empty;
                }
                CssClassRequired ??= "required";
            }

            if (dict.Count > 0) {
                this.AdditionalAttributes = dict;
            }
        }

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
