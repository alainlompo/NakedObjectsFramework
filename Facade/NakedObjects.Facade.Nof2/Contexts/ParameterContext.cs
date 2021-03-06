// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using NakedObjects.Facade.Contexts;
using org.nakedobjects.@object;

namespace NakedObjects.Facade.Nof2.Contexts {
    public class ParameterContext : Context {
        public NakedObjectActionParameter Parameter { get; set; }
        public ActionWrapper Action { get; set; }

        public override string Id {
            get { return Parameter.getId(); }
        }

        public override NakedObjectSpecification Specification {
            get { return Parameter.getSpecification(); }
        }

        public ParameterContextFacade ToParameterContextFacade(IFrameworkFacade facade) {
            var pc = new ParameterContextFacade {
                Parameter = new ActionParameterFacade(Parameter, Target, facade),
                Target = new ObjectFacade(Target, facade),
                Action = new ActionFacade(Action, Target, facade)
            };
            return ToContextFacade(pc, facade);
        }
    }
}