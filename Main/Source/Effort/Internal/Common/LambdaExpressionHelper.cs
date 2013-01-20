﻿// --------------------------------------------------------------------------------------------
// <copyright file="LambdaExpressionHelper.cs" company="Effort Team">
//     Copyright (C) 2011-2013 Effort Team
//
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
// </copyright>
// --------------------------------------------------------------------------------------------

namespace Effort.Internal.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Effort.Internal.TypeGeneration;

    internal static class LambdaExpressionHelper
    {
        public static LambdaExpression CreateSelectorExpression(Type sourceType, PropertyInfo[] selectorFields)
        {
            object selectorExpression = null;

            if (selectorFields.Length == 1)
            {
                // Single field primary key:
                //// Expression:  x => x.Field

                Type resultType = selectorFields[0].PropertyType;

                selectorExpression =
                    typeof(LambdaExpressionHelper.WrapperMethods)
                    .GetMethod("CreateSingleFieldSelector")
                    .MakeGenericMethod(sourceType, resultType)
                    .Invoke(null, new object[] { selectorFields[0].Name });
            }
            else
            {
                // Multiple field primary key:
                //// Expression: x => new { x.Field1, x.Field2 }

                // Build anonymous type
                Type resultType =
                    AnonymousTypeFactory.Create(
                        selectorFields.ToDictionary(
                            pi => pi.Name,
                            pi => pi.PropertyType));

                ////Type resultType =
                ////    TupleTypeFactory.Create(
                ////        selectorFields.Select(pi => pi.PropertyType).ToArray());

                selectorExpression =
                    typeof(LambdaExpressionHelper.WrapperMethods)
                    .GetMethod("CreateMultipleFieldSelector")
                    .MakeGenericMethod(sourceType, resultType)
                    .Invoke(null, new object[] { selectorFields.Select(pi => pi.Name) });
            }

            return selectorExpression as LambdaExpression;
        }

        public static LambdaExpression CreateInitializerExpression(Type type, PropertyInfo[] properties)
        {
            ParameterExpression[] parameters = properties.Select(p => Expression.Parameter(p.PropertyType)).ToArray();

            MemberInitExpression memberInit =
                Expression.MemberInit(
                    Expression.New(type),
                    properties.Select((p, i) => Expression.Bind(p, parameters[i])));

            LambdaExpression lambda = Expression.Lambda(memberInit, parameters);

            return lambda;
        }

        private static class WrapperMethods
        {
            public static Expression<Func<TSource, TResult>> CreateSingleFieldSelector<TSource, TResult>(string fieldName) where TSource : class
            {
                var expressionParameter = Expression.Parameter(typeof(TSource), "x");
                return
                    Expression.Lambda<Func<TSource, TResult>>(
                        Expression.Property(
                            expressionParameter,
                            typeof(TSource),
                            fieldName),
                        expressionParameter);
            }

            public static Expression<Func<TSource, TResult>> CreateMultipleFieldSelector<TSource, TResult>(IEnumerable<string> fields) where TSource : class
            {
                var expressionParameter = Expression.Parameter(typeof(TSource), "x");

                return
                    Expression.Lambda<Func<TSource, TResult>>(
                        Expression.New(
                            typeof(TResult).GetConstructors().Single(),
                            fields.Select(f => Expression.Property(expressionParameter, f)),
                            fields.Select(f => typeof(TResult).GetProperty(f))),
                        expressionParameter);
            }
        }
    }
}
