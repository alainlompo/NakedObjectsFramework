﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Spec;

namespace NakedObjects.Core.Util {
    public static class FeatureSpecUtils {
        public static string PropertyTitle(this IFeatureSpec featureSpec, INakedObject nakedObject, INakedObjectManager manager) {
            if (nakedObject == null) {
                return "";
            }
            string text = null;
            var regex = featureSpec.GetFacet<IRegExFacet>();
            if (regex != null) {
                text = regex.Format(nakedObject.TitleString());
            }
            var mask = featureSpec.GetFacet<IMaskFacet>();
            if (mask != null) {
                var title = featureSpec.Spec.GetFacet<ITitleFacet>();
                text = title.GetTitleWithMask(mask.Value, nakedObject, manager);
            }
            return text ?? (nakedObject.TitleString());
        }
    }
}