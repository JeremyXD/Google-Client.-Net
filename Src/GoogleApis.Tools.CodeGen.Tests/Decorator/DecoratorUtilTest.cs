﻿/*
Copyright 2010 Google Inc

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Tools.CodeGen.Decorator;
using NUnit.Framework;

namespace Google.Apis.Tools.CodeGen.Tests.Decorator
{
    /// <summary>
    /// Tests the DecoratorUtil class.
    /// </summary>
    [TestFixture]
    public class DecoratorUtilTest
    {
        /// <summary>
        /// Tests edge cases for the AddAutoProperty method.
        /// </summary>
        [Test]
        public void CreateAutoPropertyEdgeCaseTest()
        {
            var typeDeclaration = new CodeTypeDeclaration();
            const string name = "TestProperty";
            const string comment = "Comment";

            Assert.Throws<ArgumentNullException>(() => DecoratorUtil.CreateAutoProperty<object>(null, name, comment));
            Assert.Throws<ArgumentNullException>(
                () => DecoratorUtil.CreateAutoProperty<object>(typeDeclaration, null, comment));
            Assert.Throws<ArgumentException>(
                () => DecoratorUtil.CreateAutoProperty<object>(typeDeclaration, "", comment));
            Assert.DoesNotThrow(() => DecoratorUtil.CreateAutoProperty<object>(typeDeclaration, name, null));
        }

        /// <summary>
        /// Tests the AddAutoProperty method.
        /// </summary>
        [Test]
        public void CreateAutoPropertyTest()
        {
            var typeDeclaration = new CodeTypeDeclaration();
            const string name = "TestProperty";
            const string comment = "Comment";

            CodeTypeMemberCollection col = DecoratorUtil.CreateAutoProperty<int>(typeDeclaration, name, comment);
            Assert.That(typeDeclaration.Members.Count, Is.EqualTo(0));

            Assert.That(col.Count, Is.EqualTo(2));
            Assert.That(col[1], Is.InstanceOf<CodeMemberProperty>());
            Assert.That(col[1].Attributes, Is.EqualTo(MemberAttributes.Public));
            Assert.That(col[0], Is.InstanceOf<CodeMemberField>());

            var property = col[1] as CodeMemberProperty;
            Assert.IsNotNull(property);
            Assert.That(property.Name, Is.EqualTo(name));
            Assert.That(property.Type.BaseType, Is.EqualTo(typeof(int).FullName));

            // Backening field is tested in its own unit case.

            // Test if an exception is thrown when the name is already used.
            typeDeclaration.Members.AddRange(col);
            Assert.Throws<ArgumentException>(
                () => DecoratorUtil.CreateAutoProperty<bool>(typeDeclaration, name, comment));
        }

        /// <summary>
        /// Tests edge cases for the CreateBackingfield method.
        /// </summary>
        [Test]
        public void CreateBackingFieldEdgeCaseTest()
        {
            var typeDeclaration = new CodeTypeDeclaration();
            const string name = "TestProperty";

            Assert.Throws<ArgumentNullException>(() => DecoratorUtil.CreateBackingField<object>(null, name));
            Assert.Throws<ArgumentNullException>(
                () => DecoratorUtil.CreateBackingField<object>(typeDeclaration, null));
            Assert.Throws<ArgumentException>(() => DecoratorUtil.CreateBackingField<object>(typeDeclaration, ""));
        }

        /// <summary>
        /// Tests the generation of backing fields.
        /// </summary>
        [Test]
        public void CreateBackingFieldTest()
        {
            var typeDeclaration = new CodeTypeDeclaration();
            const string name = "TestProperty";

            CodeMemberField field = DecoratorUtil.CreateBackingField<bool>(typeDeclaration, name);
            Assert.IsNotNull(field);
            Assert.That(field.Name, Is.EqualTo("testProperty"));
            Assert.That(field.Type.BaseType, Is.EqualTo(typeof(bool).FullName));
            Assert.That(field.Attributes, Is.EqualTo(MemberAttributes.Private));
        }

        /// <summary>
        /// Tests the creation of a simple comment.
        /// </summary>
        [Test]
        public void CreateSummaryCommentTest()
        {
            string aShortDescription = "A Short description for test";
            var result = DecoratorUtil.CreateSummaryComment(aShortDescription);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.IsNotNull(result[0].Comment);
            Assert.AreEqual(true, result[0].Comment.DocComment);
            Assert.AreEqual("<summary>" + aShortDescription + "</summary>", result[0].Comment.Text);
        }
        /// <summary>
        /// Checks if empty comments can be created.
        /// </summary>
        [Test]
        public void CreateSummaryCommentEmptyCommentTest()
        {
            // Test an empty comment.
            var result = DecoratorUtil.CreateSummaryComment("");
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);

            // Test a null comment.
            result = DecoratorUtil.CreateSummaryComment(null);
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        /// <summary>
        /// Checks if xml escapes are done on non-compilant xml strings.
        /// </summary>
        [Test]
        public void CreateSummaryCommentXmlEscapeTest()
        {
            string aShortDescription = "A 'Short\" <description> <for/> test & fun";
            var result = DecoratorUtil.CreateSummaryComment(aShortDescription);
            string desiredComment =
                "<summary>A &apos;Short&quot; &lt;description&gt; &lt;for/&gt; test &amp; fun</summary>";
            Assert.AreEqual(desiredComment, result[0].Comment.Text);
        }

        /// <summary>
        /// Tests the GenerateEnum method.
        /// </summary>
        [Test]
        public void GenerateEnumTest()
        {
            const string name = "TestEnum";
            const string desc = "TestDescription";
            string[] elems = new[] { "ElemA", "ElemB", "ElemC" };
            string[] elemDescriptions = new[] { "DescA", "DescB", "DescC" };

            // Check that the method validates its inputs.
            Assert.Throws<ArgumentNullException>(
                () => DecoratorUtil.GenerateEnum(null, desc, elems, elemDescriptions));
            Assert.Throws<ArgumentNullException>(() => DecoratorUtil.GenerateEnum(name, desc, null, elemDescriptions));
            Assert.Throws<ArgumentNullException>(() => DecoratorUtil.GenerateEnum(name, desc, elems, null));

            // Check that the generated enumeration contains all elements.
            CodeTypeDeclaration decl = DecoratorUtil.GenerateEnum(name, desc, elems, elemDescriptions);
            Assert.AreEqual(name, decl.Name);
            Assert.IsTrue(decl.IsEnum);
            Assert.AreEqual(1, decl.Comments.Count);

            Assert.AreEqual(3, decl.Members.Count);
            Assert.AreEqual("ElemA", decl.Members[0].Name);
            Assert.AreEqual("ElemB", decl.Members[1].Name);
            Assert.AreEqual("ElemC", decl.Members[2].Name);

            Assert.AreEqual(1, decl.Members[0].Comments.Count);
            Assert.AreEqual(1, decl.Members[1].Comments.Count);
            Assert.AreEqual(1, decl.Members[2].Comments.Count);
        }

        /// <summary>
        /// Tests the AddMembersToClass method.
        /// </summary>
        [Test]
        public void AddMembersToClassTest()
        {
            var decl = new CodeTypeDeclaration();
            var memberA = new CodeTypeMember() { Name = "MemberA" };
            var memberB = new CodeTypeMember() { Name = "MemberB" };
            var memberC = new CodeTypeMember() { Name = "MemberA" };

            // Test parameter validation
            Assert.Throws<ArgumentNullException>(
                () => DecoratorUtil.AddMembersToClass(null, new CodeTypeMemberCollection()));
            Assert.Throws<ArgumentNullException>(() => DecoratorUtil.AddMembersToClass(decl, null));

            // Add a single element.
            DecoratorUtil.AddMembersToClass(decl, new CodeTypeMemberCollection(new[] { memberA }));
            Assert.AreEqual(1, decl.Members.Count);

            // Confirm that this element is skipped when adding it twice.
            DecoratorUtil.AddMembersToClass(decl, new CodeTypeMemberCollection(new[] { memberA }));
            Assert.AreEqual(1, decl.Members.Count);

            // Add the remaining elements
            DecoratorUtil.AddMembersToClass(decl, new CodeTypeMemberCollection(new[] { memberA, memberB, memberC }));
            Assert.AreEqual(2, decl.Members.Count);
        }

        /// <summary>
        /// Tests the GetEnumerablePairs method.
        /// </summary>
        [Test]
        public void GetEnumerablePairsTest()
        {
            string[] keys = new[] { "a", "b", "c" };
            string[] values = new[] { "1", "2", "3" };
            string[] invalidSized = new[] { "1", "2", "3", "x", "y" };

            // Test parameter validation
            Assert.Throws<ArgumentNullException>(
                () => DecoratorUtil.GetEnumerablePairs((string[]) null, values).First());
            Assert.Throws<ArgumentNullException>(
                () => DecoratorUtil.GetEnumerablePairs(keys, (string[]) null).First());

            Assert.Throws<ArgumentException>(() => DecoratorUtil.GetEnumerablePairs(keys, invalidSized).First());
            Assert.Throws<ArgumentException>(() => DecoratorUtil.GetEnumerablePairs(invalidSized, values).First());

            // Test simple operation
            IEnumerable<KeyValuePair<string, string>> joined = DecoratorUtil.GetEnumerablePairs(keys, values);
            Assert.AreEqual(3, joined.Count());
            
            foreach (KeyValuePair<string,string> pair in joined)
            {
                Assert.AreEqual(pair.Key[0] - 'a', pair.Value[0] - '1');
            }
        }
    }
}