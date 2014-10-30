// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using Common.Logging;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.FacetFactory;
using NakedObjects.Architecture.Facets.Types;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.Spec;
using NakedObjects.Meta.Utils;

namespace NakedObjects.Reflect.FacetFactory {
    public class TypeMarkerFacetFactory : AnnotationBasedFacetFactoryAbstract {
        private static readonly ILog Log = LogManager.GetLogger(typeof (DefaultNamingFacetFactory));

        public TypeMarkerFacetFactory(IReflector reflector)
            : base(reflector, FeatureType.Objects) {}

        public override bool Process(Type type, IMethodRemover methodRemover, ISpecificationBuilder specification) {
            var facets = new List<IFacet>();

            if (IsAbstract(type)) {
                facets.Add(new AbstractFacet(specification));
            }

            if (IsInterface(type)) {
                facets.Add(new InterfaceFacet(specification));
            }

            if (IsSealed(type)) {
                facets.Add(new SealedFacet(specification));
            }

            if (IsVoid(type)) {
                facets.Add(new VoidFacet(specification));
            }

            return FacetUtils.AddFacets(facets);
        }

        private static bool IsVoid(Type type) {
            return type == typeof (void);
        }

        private static bool IsSealed(Type type) {
            return type.IsSealed;
        }

        private static bool IsInterface(Type type) {
            return type.IsInterface;
        }

        private static bool IsAbstract(Type type) {
            return type.IsAbstract;
        }
    }
}