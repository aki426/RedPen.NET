﻿//   Copyright (c) 2024 KANEDA Akihiro <taoist.aki@gmail.com>
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;

namespace RedPen.Net.Core
{
    public record ValidationDefinition
    {
        public string ValidationName { get; init; }
        public Type ValidatorConfigurationType { get; init; }
        public Type ValidatorType { get; init; }

        public static ValidationDefinition RegisterType<TValidatorConfiguration, TValidator>()
        {
            return new ValidationDefinition
            {
                ValidationName = typeof(TValidator).Name.Substring(0, typeof(TValidator).Name.Length - "Validator".Length),
                ValidatorConfigurationType = typeof(TValidatorConfiguration),
                ValidatorType = typeof(TValidator)
            };
        }
    }
}
