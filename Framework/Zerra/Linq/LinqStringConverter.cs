﻿// Copyright © KaKush LLC
// Written By Steven Zawaski
// Licensed to you under the MIT license

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Zerra.Reflection;

namespace Zerra.Linq
{
    public static partial class LinqStringConverter
    {
        public static string Convert(Expression exp, bool useIt)
        {
            var sb = new StringBuilder();

            var context = new ConvertContext(sb, useIt);

            ConvertToString(exp, context);

            return sb.ToString();
        }

        private static void ConvertToString(Expression exp, ConvertContext context)
        {
            switch (exp.NodeType)
            {
                case ExpressionType.Add:
                    ConvertToStringBinary("+", exp, context);
                    break;
                case ExpressionType.AddAssign:
                    ConvertToStringBinary("+=", exp, context);
                    break;
                case ExpressionType.AddAssignChecked:
                    ConvertToStringBinary("+=", exp, context);
                    break;
                case ExpressionType.AddChecked:
                    ConvertToStringBinary("+", exp, context);
                    break;
                case ExpressionType.And:
                    ConvertToStringBinary("&", exp, context);
                    break;
                case ExpressionType.AndAlso:
                    ConvertToStringBinary("&&", exp, context);
                    break;
                case ExpressionType.AndAssign:
                    ConvertToStringBinary("&=", exp, context);
                    break;
                case ExpressionType.ArrayLength:
                    throw new NotImplementedException();
                case ExpressionType.Assign:
                    throw new NotImplementedException();
                case ExpressionType.Block:
                    throw new NotImplementedException();
                case ExpressionType.Call:
                    ConvertToStringCall(exp, context);
                    break;
                case ExpressionType.Coalesce:
                    throw new NotImplementedException();
                case ExpressionType.Conditional:
                    ConvertToStringConditional(exp, context);
                    break;
                case ExpressionType.Constant:
                    ConvertToStringConstant(exp, context);
                    break;
                case ExpressionType.Convert:
                    ConvertToStringUnary(null, null, exp, context);
                    break;
                case ExpressionType.ConvertChecked:
                    ConvertToStringUnary(null, null, exp, context);
                    break;
                case ExpressionType.DebugInfo:
                    throw new NotImplementedException();
                case ExpressionType.Decrement:
                    ConvertToStringUnary(null, "--", exp, context);
                    break;
                case ExpressionType.Default:
                    throw new NotImplementedException();
                case ExpressionType.Divide:
                    ConvertToStringBinary("/", exp, context);
                    break;
                case ExpressionType.DivideAssign:
                    ConvertToStringBinary("/=", exp, context);
                    break;
                case ExpressionType.Dynamic:
                    throw new NotImplementedException();
                case ExpressionType.Equal:
                    ConvertToStringBinary("=", exp, context);
                    break;
                case ExpressionType.ExclusiveOr:
                    ConvertToStringBinary("^", exp, context);
                    break;
                case ExpressionType.ExclusiveOrAssign:
                    ConvertToStringBinary("^=", exp, context);
                    break;
                case ExpressionType.Extension:
                    throw new NotImplementedException();
                case ExpressionType.Goto:
                    throw new NotImplementedException();
                case ExpressionType.GreaterThan:
                    ConvertToStringBinary(">", exp, context);
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    ConvertToStringBinary(">=", exp, context);
                    break;
                case ExpressionType.Increment:
                    ConvertToStringUnary(null, "++", exp, context);
                    break;
                case ExpressionType.Index:
                    throw new NotImplementedException();
                case ExpressionType.Invoke:
                    throw new NotImplementedException();
                case ExpressionType.IsFalse:
                    throw new NotImplementedException();
                case ExpressionType.IsTrue:
                    throw new NotImplementedException();
                case ExpressionType.Label:
                    throw new NotImplementedException();
                case ExpressionType.Lambda:
                    ConvertToStringLambda(exp, context);
                    break;
                case ExpressionType.LeftShift:
                    ConvertToStringBinary("<<", exp, context);
                    break;
                case ExpressionType.LeftShiftAssign:
                    ConvertToStringBinary("<<=", exp, context);
                    break;
                case ExpressionType.LessThan:
                    ConvertToStringBinary("<", exp, context);
                    break;
                case ExpressionType.LessThanOrEqual:
                    ConvertToStringBinary("<=", exp, context);
                    break;
                case ExpressionType.ListInit:
                    throw new NotImplementedException();
                case ExpressionType.Loop:
                    throw new NotImplementedException();
                case ExpressionType.MemberAccess:
                    ConvertToStringMember(exp, context);
                    break;
                case ExpressionType.MemberInit:
                    throw new NotImplementedException();
                case ExpressionType.Modulo:
                    ConvertToStringBinary("%", exp, context);
                    break;
                case ExpressionType.ModuloAssign:
                    ConvertToStringBinary("%=", exp, context);
                    break;
                case ExpressionType.Multiply:
                    ConvertToStringBinary("*", exp, context);
                    break;
                case ExpressionType.MultiplyAssign:
                    ConvertToStringBinary("*=", exp, context);
                    break;
                case ExpressionType.MultiplyAssignChecked:
                    ConvertToStringBinary("*=", exp, context);
                    break;
                case ExpressionType.MultiplyChecked:
                    ConvertToStringBinary("*", exp, context);
                    break;
                case ExpressionType.Negate:
                    ConvertToStringUnary("-", null, exp, context);
                    break;
                case ExpressionType.NegateChecked:
                    ConvertToStringUnary("-", null, exp, context);
                    break;
                case ExpressionType.New:
                    ConvertToStringNew(exp, context);
                    break;
                case ExpressionType.NewArrayBounds:
                    throw new NotImplementedException();
                case ExpressionType.NewArrayInit:
                    throw new NotImplementedException();
                case ExpressionType.Not:
                    ConvertToStringUnary("!", null, exp, context);
                    break;
                case ExpressionType.NotEqual:
                    ConvertToStringBinary("!=", exp, context);
                    break;
                case ExpressionType.OnesComplement:
                    ConvertToStringUnary("~", null, exp, context);
                    break;
                case ExpressionType.Or:
                    ConvertToStringBinary("|", exp, context);
                    break;
                case ExpressionType.OrAssign:
                    ConvertToStringBinary("|=", exp, context);
                    break;
                case ExpressionType.OrElse:
                    ConvertToStringBinary("||", exp, context);
                    break;
                case ExpressionType.Parameter:
                    ConvertToStringParameter(exp, context);
                    break;
                case ExpressionType.PostDecrementAssign:
                    ConvertToStringUnary(null, "--", exp, context);
                    break;
                case ExpressionType.PostIncrementAssign:
                    ConvertToStringUnary(null, "++", exp, context);
                    break;
                case ExpressionType.Power:
                    ConvertToStringBinary("^", exp, context);
                    break;
                case ExpressionType.PowerAssign:
                    ConvertToStringBinary("^=", exp, context);
                    break;
                case ExpressionType.PreDecrementAssign:
                    ConvertToStringUnary("--", null, exp, context);
                    break;
                case ExpressionType.PreIncrementAssign:
                    ConvertToStringUnary("++", null, exp, context);
                    break;
                case ExpressionType.Quote:
                    throw new NotImplementedException();
                case ExpressionType.RightShift:
                    ConvertToStringBinary(">>", exp, context);
                    break;
                case ExpressionType.RightShiftAssign:
                    ConvertToStringBinary(">>=", exp, context);
                    break;
                case ExpressionType.RuntimeVariables:
                    throw new NotImplementedException();
                case ExpressionType.Subtract:
                    ConvertToStringBinary("-", exp, context);
                    break;
                case ExpressionType.SubtractAssign:
                    ConvertToStringBinary("-=", exp, context);
                    break;
                case ExpressionType.SubtractAssignChecked:
                    ConvertToStringBinary("-=", exp, context);
                    break;
                case ExpressionType.SubtractChecked:
                    ConvertToStringBinary("-", exp, context);
                    break;
                case ExpressionType.Switch:
                    throw new NotImplementedException();
                case ExpressionType.Throw:
                    throw new NotImplementedException();
                case ExpressionType.Try:
                    throw new NotImplementedException();
                case ExpressionType.TypeAs:
                    throw new NotImplementedException();
                case ExpressionType.TypeEqual:
                    throw new NotImplementedException();
                case ExpressionType.TypeIs:
                    throw new NotImplementedException();
                case ExpressionType.UnaryPlus:
                    throw new NotImplementedException();
                case ExpressionType.Unbox:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }
        private static void ConvertToStringLambda(Expression exp, ConvertContext context)
        {
            var lambda = exp as LambdaExpression;
            if (lambda.Parameters.Count != 1)
                throw new NotSupportedException("Can only parse a lambda with one parameter.");
            if (context.ItStack.Count > 0 && context.ItStack.Peek() == null)
                context.ItStack.Pop();
            context.ItStack.Push(lambda.Parameters[0].Name);
            ConvertToString(lambda.Body, context);
            context.ItStack.Pop();
        }
        private static void ConvertToStringUnary(string prefixOperation, string suffixOperation, Expression exp, ConvertContext context)
        {
            var unary = exp as UnaryExpression;
            context.Builder.Append(prefixOperation);
            ConvertToString(unary.Operand, context);
            context.Builder.Append(suffixOperation);
        }
        private static void ConvertToStringBinary(string operation, Expression exp, ConvertContext context)
        {
            var binary = exp as BinaryExpression;
            context.Builder.Append('(');
            ConvertToString(binary.Left, context);
            context.Builder.Append(')');
            context.Builder.Append(operation);
            context.Builder.Append('(');
            ConvertToString(binary.Right, context);
            context.Builder.Append(')');
        }
        private static void ConvertToStringMember(Expression exp, ConvertContext context)
        {
            var member = exp as MemberExpression;

            if (member.Expression == null)
            {
                ConvertToStringEvaluate(member, context);
            }
            else
            {
                context.MemberAccessStack.Push(member);
                ConvertToString(member.Expression, context);
                context.MemberAccessStack.Pop();
            }
        }
        private static void ConvertToStringConstant(Expression exp, ConvertContext context)
        {
            var constant = exp as ConstantExpression;

            ConvertToStringConstantStack(constant.Type, constant.Value, context);
        }
        private static void ConvertToStringConstantStack(Type type, object value, ConvertContext context)
        {
            if (context.MemberAccessStack.Count > 0)
            {
                var memberProperty = context.MemberAccessStack.Pop();

                switch (memberProperty.Member.MemberType)
                {
                    case MemberTypes.Field:
                        {
                            var field = memberProperty.Member as FieldInfo;
                            object fieldValue = field.GetValue(value);
                            ConvertToStringConstantStack(field.FieldType, fieldValue, context);
                            break;
                        }
                    case MemberTypes.Property:
                        {
                            var property = memberProperty.Member as PropertyInfo;
                            object propertyValue = property.GetValue(value);
                            ConvertToStringConstantStack(property.PropertyType, propertyValue, context);
                            break;
                        }
                    default:
                        throw new NotImplementedException();
                }

                context.MemberAccessStack.Push(memberProperty);
            }
            else
            {
                ConvertToStringValue(type, value, context);
            }
        }
        private static void ConvertToStringCall(Expression exp, ConvertContext context)
        {
            var call = exp as MethodCallExpression;
            bool isEvaluatable = IsEvaluatable(exp);
            if (isEvaluatable)
            {
                ConvertToStringEvaluate(exp, context);
            }
            else
            {
                if (call.Object == null)
                {
                    var callingObject = call.Arguments[0];
                    ConvertToString(callingObject, context);
                    context.Builder.Append('.');
                    context.Builder.Append(call.Method.Name);
                    context.Builder.Append('(');
                    context.ItStack.Push(null);
                    for (int i = 1; i < call.Arguments.Count; i++)
                    {
                        if (i > 1)
                            context.Builder.Append(',');
                        var arg = call.Arguments[i];
                        ConvertToString(arg, context);
                    }
                    if (context.ItStack.Peek() == null)
                        context.ItStack.Pop();
                    context.Builder.Append(')');
                }
                else
                {
                    ConvertToString(call.Object, context);
                    context.Builder.Append('.');
                    context.Builder.Append(call.Method.Name);
                    context.Builder.Append('(');
                    context.ItStack.Push(null);
                    for (int i = 0; i < call.Arguments.Count; i++)
                    {
                        if (i > 1)
                            context.Builder.Append(',');
                        var arg = call.Arguments[i];
                        ConvertToString(arg, context);
                    }
                    if (context.ItStack.Peek() == null)
                        context.ItStack.Pop();
                    context.Builder.Append(')');
                }
            }
        }
        private static void ConvertToStringNew(Expression exp, ConvertContext context)
        {
            var newExp = exp as NewExpression;

            var argumentTypes = newExp.Arguments.Select(x => x.Type).ToArray();
            var constructor = newExp.Type.GetConstructor(argumentTypes);

            List<object> parameters = new List<object>();
            foreach (var argument in newExp.Arguments)
            {
                var argumentValue = Evaluate(argument);
                parameters.Add(argumentValue);
            }

            var value = constructor.Invoke(parameters.ToArray());
            ConvertToStringValue(newExp.Type, value, context);
        }
        private static void ConvertToStringParameter(Expression exp, ConvertContext context)
        {
            var parameterExpression = exp as ParameterExpression;

            if (context.UseIt)
            {
                if (parameterExpression.Name == context.ItStack.Peek())
                {
                    context.Builder.Append("it.");
                }
                else
                {
                    int rank = 0;
                    foreach (var pstack in context.ItStack)
                    {
                        if (pstack == parameterExpression.Name)
                            break;
                        rank++;
                    }
                    context.Builder.Append("oIt").Append(rank).Append('.');
                }
            }

            ConvertToStringParameterStack(context);
        }

        private static void ConvertToStringParameterStack(ConvertContext context)
        {
            if (context.MemberAccessStack.Count > 0)
            {
                var member = context.MemberAccessStack.Pop();

                context.Builder.Append(member.Member.Name);
                if (context.MemberAccessStack.Count > 0)
                    context.Builder.Append('.');

                ConvertToStringParameterStack(context);

                context.MemberAccessStack.Push(member);
            }
        }
        private static void ConvertToStringConditional(Expression exp, ConvertContext context)
        {
            var conditional = exp as ConditionalExpression;
            ConvertToString(conditional.Test, context);
            context.Builder.Append('?');
            ConvertToString(conditional.IfTrue, context);
            context.Builder.Append(':');
            ConvertToString(conditional.IfFalse, context);
        }
        private static void ConvertToStringEvaluate(Expression exp, ConvertContext context)
        {
            var value = Evaluate(exp);
            ConvertToStringValue(exp.Type, value, context);
        }

        private static void ConvertToStringValue(Type type, object value, ConvertContext context)
        {
            ConvertToStringValueRender(type, value, context);

            ConvertToStringValueStack(context);
        }
        private static void ConvertToStringValueRender(Type type, object value, ConvertContext context)
        {
            var typeDetails = TypeAnalyzer.GetTypeDetail(type);

            if (value == null)
            {
                context.Builder.Append("null");
                ConvertToStringValueStack(context);
                return;
            }

            if (typeDetails.IsNullable)
            {
                type = typeDetails.InnerTypes[0];
                typeDetails = typeDetails.InnerTypeDetails[0];
            }

            if (type.IsEnum)
            {
                context.Builder.Append(type.Name).Append('.').Append(value);
                return;
            }

            if (type.IsArray)
            {
                Type arrayType = typeDetails.InnerTypes[0];
                context.Builder.Append('[');
                bool first = true;
                foreach (object item in (IEnumerable)value)
                {
                    if (!first)
                        context.Builder.Append(',');
                    ConvertToStringValue(arrayType, item, context);
                    first = false;
                }
                context.Builder.Append(']');
                return;
            }

            if (typeDetails.IsIEnumerableGeneric)
            {
                context.Builder.Append('[');
                bool first = true;
                foreach (object item in (IEnumerable)value)
                {
                    if (!first)
                        context.Builder.Append(',');
                    ConvertToStringValue(typeDetails.IEnumerableGenericInnerType, item, context);
                    first = false;
                }
                context.Builder.Append(']');
                return;
            }

            if (TypeLookup.CoreTypeLookup(type, out CoreType coreType))
            {
                switch (coreType)
                {
                    case CoreType.Boolean: context.Builder.Append((bool)value); return;
                    case CoreType.Byte: context.Builder.Append((byte)value); return;
                    case CoreType.SByte: context.Builder.Append((sbyte)value); return;
                    case CoreType.Int16: context.Builder.Append((short)value); return;
                    case CoreType.UInt16: context.Builder.Append((ushort)value); return;
                    case CoreType.Int32: context.Builder.Append((int)value); return;
                    case CoreType.UInt32: context.Builder.Append((uint)value); return;
                    case CoreType.Int64: context.Builder.Append((long)value); return;
                    case CoreType.UInt64: context.Builder.Append((ulong)value); return;
                    case CoreType.Single: context.Builder.Append((float)value); return;
                    case CoreType.Double: context.Builder.Append((double)value); return;
                    case CoreType.Decimal: context.Builder.Append((decimal)value); return;
                    case CoreType.Char: context.Builder.Append('\'').Append(value).Append('\''); return;
                    case CoreType.DateTime: context.Builder.Append("DateTime.Parse(\"").Append(value).Append("\")"); return;
                    case CoreType.DateTimeOffset: context.Builder.Append("DateTimeOffset.Parse(\"").Append(value).Append("\")"); return;
                    case CoreType.TimeSpan: context.Builder.Append("TimeSpan.Parse(\"").Append(value).Append("\")"); return;
                    case CoreType.Guid: context.Builder.Append('\'').Append(value.ToString()).Append('\''); return;
                    case CoreType.String: context.Builder.Append('\'').Append(((string)value).Replace("'", "''")).Append('\''); return;
                }
            }

            if (type == typeof(object))
            {
                context.Builder.Append('\'').Append(value.ToString().Replace("'", "''")).Append('\'');
                return;
            }

            throw new NotImplementedException($"{type.GetNiceName()} value {value?.ToString()} not converted");
        }
        private static void ConvertToStringValueStack(ConvertContext context)
        {
            if (context.MemberAccessStack.Count > 0)
            {
                var member = context.MemberAccessStack.Pop();

                context.Builder.Append('.');
                context.Builder.Append(member.Member.Name);

                ConvertToStringParameterStack(context);

                context.MemberAccessStack.Push(member);
            }
        }

        private static bool IsEvaluatable(Expression exp)
        {
            return exp.NodeType switch
            {
                ExpressionType.Add => IsEvaluatableBinary(exp),
                ExpressionType.AddAssign => IsEvaluatableBinary(exp),
                ExpressionType.AddAssignChecked => IsEvaluatableBinary(exp),
                ExpressionType.AddChecked => IsEvaluatableBinary(exp),
                ExpressionType.And => IsEvaluatableBinary(exp),
                ExpressionType.AndAlso => IsEvaluatableBinary(exp),
                ExpressionType.AndAssign => IsEvaluatableBinary(exp),
                ExpressionType.ArrayIndex => throw new NotImplementedException(),
                ExpressionType.ArrayLength => IsEvaluatableUnary(exp),
                ExpressionType.Assign => IsEvaluatableBinary(exp),
                ExpressionType.Block => IsEvaluatableBlock(exp),
                ExpressionType.Call => IsEvaluatableCall(exp),
                ExpressionType.Coalesce => throw new NotImplementedException(),
                ExpressionType.Conditional => throw new NotImplementedException(),
                ExpressionType.Constant => true,
                ExpressionType.Convert => IsEvaluatableUnary(exp),
                ExpressionType.ConvertChecked => throw new NotImplementedException(),
                ExpressionType.DebugInfo => throw new NotImplementedException(),
                ExpressionType.Decrement => IsEvaluatableUnary(exp),
                ExpressionType.Default => throw new NotImplementedException(),
                ExpressionType.Divide => IsEvaluatableBinary(exp),
                ExpressionType.DivideAssign => IsEvaluatableBinary(exp),
                ExpressionType.Dynamic => throw new NotImplementedException(),
                ExpressionType.Equal => IsEvaluatableBinary(exp),
                ExpressionType.ExclusiveOr => IsEvaluatableBinary(exp),
                ExpressionType.ExclusiveOrAssign => IsEvaluatableBinary(exp),
                ExpressionType.Extension => throw new NotImplementedException(),
                ExpressionType.Goto => throw new NotImplementedException(),
                ExpressionType.GreaterThan => throw new NotImplementedException(),
                ExpressionType.GreaterThanOrEqual => throw new NotImplementedException(),
                ExpressionType.Increment => IsEvaluatableUnary(exp),
                ExpressionType.Index => throw new NotImplementedException(),
                ExpressionType.Invoke => throw new NotImplementedException(),
                ExpressionType.IsFalse => throw new NotImplementedException(),
                ExpressionType.IsTrue => throw new NotImplementedException(),
                ExpressionType.Label => throw new NotImplementedException(),
                ExpressionType.Lambda => throw new NotImplementedException(),
                ExpressionType.LeftShift => IsEvaluatableBinary(exp),
                ExpressionType.LeftShiftAssign => IsEvaluatableBinary(exp),
                ExpressionType.LessThan => throw new NotImplementedException(),
                ExpressionType.LessThanOrEqual => throw new NotImplementedException(),
                ExpressionType.ListInit => throw new NotImplementedException(),
                ExpressionType.Loop => throw new NotImplementedException(),
                ExpressionType.MemberAccess => IsEvaluatableMemberAccess(exp),
                ExpressionType.MemberInit => throw new NotImplementedException(),
                ExpressionType.Modulo => IsEvaluatableBinary(exp),
                ExpressionType.ModuloAssign => IsEvaluatableBinary(exp),
                ExpressionType.Multiply => IsEvaluatableBinary(exp),
                ExpressionType.MultiplyAssign => IsEvaluatableBinary(exp),
                ExpressionType.MultiplyAssignChecked => IsEvaluatableBinary(exp),
                ExpressionType.MultiplyChecked => IsEvaluatableBinary(exp),
                ExpressionType.Negate => IsEvaluatableUnary(exp),
                ExpressionType.NegateChecked => IsEvaluatableUnary(exp),
                ExpressionType.New => throw new NotImplementedException(),
                ExpressionType.NewArrayBounds => throw new NotImplementedException(),
                ExpressionType.NewArrayInit => throw new NotImplementedException(),
                ExpressionType.Not => IsEvaluatableUnary(exp),
                ExpressionType.NotEqual => IsEvaluatableBinary(exp),
                ExpressionType.OnesComplement => IsEvaluatableUnary(exp),
                ExpressionType.Or => IsEvaluatableBinary(exp),
                ExpressionType.OrAssign => IsEvaluatableBinary(exp),
                ExpressionType.OrElse => IsEvaluatableBinary(exp),
                ExpressionType.Parameter => false,
                ExpressionType.PostDecrementAssign => IsEvaluatableUnary(exp),
                ExpressionType.PostIncrementAssign => IsEvaluatableUnary(exp),
                ExpressionType.Power => IsEvaluatableBinary(exp),
                ExpressionType.PowerAssign => IsEvaluatableBinary(exp),
                ExpressionType.PreDecrementAssign => IsEvaluatableUnary(exp),
                ExpressionType.PreIncrementAssign => IsEvaluatableUnary(exp),
                ExpressionType.Quote => throw new NotImplementedException(),
                ExpressionType.RightShift => IsEvaluatableBinary(exp),
                ExpressionType.RightShiftAssign => IsEvaluatableBinary(exp),
                ExpressionType.RuntimeVariables => throw new NotImplementedException(),
                ExpressionType.Subtract => IsEvaluatableBinary(exp),
                ExpressionType.SubtractAssign => IsEvaluatableBinary(exp),
                ExpressionType.SubtractAssignChecked => IsEvaluatableBinary(exp),
                ExpressionType.SubtractChecked => IsEvaluatableBinary(exp),
                ExpressionType.Switch => throw new NotImplementedException(),
                ExpressionType.Throw => throw new NotImplementedException(),
                ExpressionType.Try => throw new NotImplementedException(),
                ExpressionType.TypeAs => IsEvaluatableUnary(exp),
                ExpressionType.TypeEqual => IsEvaluatableUnary(exp),
                ExpressionType.TypeIs => IsEvaluatableUnary(exp),
                ExpressionType.UnaryPlus => IsEvaluatableUnary(exp),
                ExpressionType.Unbox => IsEvaluatableUnary(exp),
                _ => throw new NotImplementedException(),
            };
            ;
        }
        private static bool IsEvaluatableUnary(Expression exp)
        {
            var unary = exp as UnaryExpression;
            return IsEvaluatable(unary.Operand);
        }
        private static bool IsEvaluatableBinary(Expression exp)
        {
            var binary = exp as BinaryExpression;
            return IsEvaluatable(binary.Left) && IsEvaluatable(binary.Right);
        }
        private static bool IsEvaluatableBlock(Expression exp)
        {
            var block = exp as BlockExpression;
            foreach (var variable in block.Variables)
                if (!IsEvaluatable(variable))
                    return false;
            foreach (var expression in block.Expressions)
                if (!IsEvaluatable(expression))
                    return false;
            return true;
        }
        private static bool IsEvaluatableCall(Expression exp)
        {
            var call = exp as MethodCallExpression;

            foreach (var arg in call.Arguments)
                if (!IsEvaluatable(arg))
                    return false;

            if (call.Object != null)
                return IsEvaluatable(call.Object);

            return true;
        }
        private static bool IsEvaluatableMemberAccess(Expression exp)
        {
            var member = exp as MemberExpression;
            if (member.Expression == null)
            {
                return true;
            }
            return IsEvaluatable(member.Expression);
        }

        private static object Evaluate(Expression exp)
        {
            return exp.NodeType switch
            {
                ExpressionType.Constant => EvaluateConstant(exp),
                ExpressionType.MemberAccess => EvaluateMemberAccess(exp),
                _ => EvaluateInvoke(exp),
            };
        }
        private static object EvaluateConstant(Expression exp)
        {
            var constant = exp as ConstantExpression;
            return constant.Value;
        }
        private static object EvaluateMemberAccess(Expression exp)
        {
            var member = exp as MemberExpression;
            var expressionValue = member.Expression == null ? null : Evaluate(member.Expression);

            object value;
            switch (member.Member.MemberType)
            {
                case MemberTypes.Field:
                    var fieldInfo = (FieldInfo)member.Member;
                    if (expressionValue == null && !fieldInfo.IsStatic)
                        return null;
                    value = fieldInfo.GetValue(expressionValue);
                    break;
                case MemberTypes.Property:
                    var propertyInfo = (PropertyInfo)member.Member;
                    if (expressionValue == null && !propertyInfo.GetMethod.IsStatic)
                        return null;
                    value = propertyInfo.GetValue(expressionValue);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return value;
        }
        private static object EvaluateInvoke(Expression exp)
        {
            var value = Expression.Lambda(exp).Compile().DynamicInvoke();
            return value;
        }
    }
}
