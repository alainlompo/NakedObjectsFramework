// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Core.Configuration;
using NakedObjects.Reflector.DotNet.Reflect.Strategy;
using NakedObjects.Reflector.FacetFactory;
using NakedObjects.Reflector.Spec;
using NUnit.Framework;

namespace NakedObjects.Reflector.DotNet.Reflect {
    public abstract class AbstractDotNetReflectorTest {
        protected IMetamodel Metamodel;
        protected IObjectSpecImmutable Specification;

        [SetUp]
        public virtual void SetUp() {
            var classStrategy = new DefaultClassStrategy();
            var metamodel = new Metamodel(classStrategy);
            var config = new ReflectorConfiguration(new Type[] { }, new Type[] { }, new Type[] { }, new Type[] { });
            var servicesConfig = new ServicesConfiguration();
            var reflector = new DotNetReflector(classStrategy, new FacetFactorySet(), new FacetDecoratorSet(), metamodel, config, servicesConfig);

            Specification = LoadSpecification(reflector);
            //reflector.PopulateAssociatedActions(Specification, new Type[] {});
            Metamodel = metamodel;
        }

        protected abstract IObjectSpecImmutable LoadSpecification(DotNetReflector reflector);
    }

    // Copyright (c) Naked Objects Group Ltd.
}