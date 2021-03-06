﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NakedObjects.Boot;
using NakedObjects.Core.NakedObjectsSystem;
using NakedObjects.EntityObjectStore;
using NakedObjects.Helpers.Test.ViewModel;
using NakedObjects.Services;
using NakedObjects.Xat;
using NakedObjects.Helpers.Test;

namespace NakedObjects.SystemTest.ObjectFinderCompoundKeys {

    [TestClass]
    public abstract class TestObjectFinderWithCompoundKeysAbstract : AbstractHelperTest<PaymentContext> {
        protected ITestObject customer1;
        protected ITestObject customer2a;
        protected ITestObject customer2b;
        protected ITestObject customer3;
        protected ITestObject emp1;
        protected ITestProperty key1;
        protected ITestProperty payee1;
        protected ITestObject payment1;
        protected ITestObject supplier1;


        [TestInitialize]
        public void Initialize() {
            StartTest();
            payment1 = GetAllInstances("Payments", 0);
            payee1 = payment1.GetPropertyByName("Payee");
            key1 = payment1.GetPropertyByName("Payee Compound Key");

            customer1 = GetAllInstances("Customer Ones", 0);
            customer2a = GetAllInstances("Customer Twos", 0);
            customer2b = GetAllInstances("Customer Twos", 1);
            customer3 = GetAllInstances("Customer Threes", 0);
            supplier1 = GetAllInstances("Suppliers", 0);
            emp1 = GetAllInstances("Employees", 0);
        }

        [TestCleanup]
        public void CleanUp() {
            payment1 = null;
            customer1 = null;
            customer2a = null;
            customer2b = null;
            customer3 = null;
            payee1 = null;
            key1 = null;
            emp1 = null;
        }
    }
}