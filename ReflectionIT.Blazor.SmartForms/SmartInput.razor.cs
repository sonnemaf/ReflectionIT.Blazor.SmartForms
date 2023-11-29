using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace ReflectionIT.Blazor.SmartForms {

    partial class SmartInput<TItem> : InputBase<TItem> {

        private const string DefaultCssClassDiv = "";
        private const string DefaultCssClassLabel = "form-label";
        private const string DefaultCssClassInput = "form-control";
        private const string DefaultCssClassValidation = "text-danger";
        private const string DefaultCssClassRequired = "required";

        [Parameter] public int? LabelColumnSizeMedium { get; set; }
        [Parameter] public int? LabelColumSizeLarge { get; set; }

        [Parameter] public string CssClassDiv { get; set; } = DefaultCssClassDiv;
        [Parameter] public string CssClassLabel { get; set; } = DefaultCssClassLabel;
        [Parameter] public string CssClassInput { get; set; } = DefaultCssClassInput;
        [Parameter] public string CssClassValidation { get; set; } = DefaultCssClassValidation;
        [Parameter] public string? CssClassRequired { get; set; }

        [Parameter] public bool DisplayLabel { get; set; } = true;
        [Parameter] public bool DisplayValidationMessage { get; set; } = true;

        [Parameter] public string? Id { get; set; }
        [Parameter] public string? Prefix { get; set; }
        [Parameter] public string? Suffix { get; set; }

        protected bool IsCheckbox { get; private set; }

        protected bool IsDate { get; private set; }

        protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out TItem result, [NotNullWhen(false)] out string? validationErrorMessage) {
            // Debug.WriteLine($"TryParseValueFromString {value}");
            // Let's Blazor convert the value for us
            if (BindConverter.TryConvertTo(value, System.Globalization.CultureInfo.CurrentCulture, out TItem? parsedValue)) {
                result = parsedValue!;
                validationErrorMessage = null;
                return true;
            }

            result = default;
            validationErrorMessage = $"The {FieldIdentifier.FieldName} field is not valid.";
            return false;
        }
        protected string GetDisplayName() {
            if (!string.IsNullOrEmpty(DisplayName)) {
                return DisplayName;
            }
            var expression = (MemberExpression?)ValueExpression?.Body;
            var value = expression?.Member.GetCustomAttribute<DisplayAttribute>();
            return value?.Name ?? expression?.Member.Name ?? string.Empty;
        }

        protected string? GetDisplayName(object value) {
            // Read the Display attribute name
            var member = value.GetType().GetMember(value.ToString()!).ElementAtOrDefault(0);
            var displayAttribute = member?.GetCustomAttribute<DisplayAttribute>();
            return displayAttribute is not null ? displayAttribute.GetName() : value.ToString();
        }

        protected string GetId() {
            var expression = (MemberExpression?)ValueExpression?.Body;
            return (expression?.Member.Name ?? string.Empty).ToLower();
        }

        protected bool IsRow => this.LabelColumnSizeMedium.HasValue || this.LabelColumSizeLarge.HasValue;

        protected override void OnParametersSet() {
            var dict = this.AdditionalAttributes is null ? new Dictionary<string, object>() : new Dictionary<string, object>(this.AdditionalAttributes, StringComparer.OrdinalIgnoreCase);
            base.OnParametersSet();

            if (ValueExpression is null) {
                return;
            }

            var type = typeof(TItem);
            if (IsNumericType(type)) {
                dict["type"] = "number";
            }
            if (type == typeof(DateTime) || type == typeof(DateTime?) || type == typeof(DateOnly) || type == typeof(DateOnly?)) {
                IsDate = true;
            }
            if (type == typeof(bool)) {
                IsCheckbox = true;
                if (CssClassInput == DefaultCssClassInput) {
                    CssClassInput = "form-check-input";
                }
                if (CssClassLabel == DefaultCssClassLabel) {
                    CssClassLabel = "form-check-label";
                }
            }

            if (type.IsEnum) {
                if (CssClassInput == DefaultCssClassInput) {
                    CssClassInput = "form-select";
                }
            }

            var expression = (MemberExpression)ValueExpression.Body;

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

            var requiredAttribute = expression.Member.GetCustomAttribute<RequiredAttribute>();
            if (requiredAttribute is not null || (type.IsValueType && !(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)))) {
                if (!dict.ContainsKey(DefaultCssClassRequired)) {
                    dict[DefaultCssClassRequired] = string.Empty;
                }
                CssClassRequired ??= DefaultCssClassRequired;
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
                        return IsNumericType(Nullable.GetUnderlyingType(type)!);
                    }
                    return false;
            }
            return false;
        }
    }
}
