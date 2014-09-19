// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

namespace NakedObjects.Capabilities {
    /// <summary>
    ///     Provides a mechanism for encoding/decoding objects
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This interface is used in two complementary ways
    ///     </para>
    ///     <list type="number">
    ///         <item>
    ///             As one option, it allows objects to
    ///             take control of their own encoding/decoding, by implementing directly.
    ///             However, the instance is used as a factory for itself.  The framework will instantiate
    ///             an instance, invoke the appropriate method method, and use the returned object.
    ///             The instantiated instance itself will be discarded.
    ///         </item>
    ///         <item>
    ///             Alternatively, an implementor of this interface can be
    ///             nominated in the <see cref="EncodeableAttribute" /> annotation,
    ///             allowing a class that needs to be encodeable to indicate how it can be encoded/decoded
    ///         </item>
    ///     </list>
    ///     <para>
    ///         Whatever the class that implements this interface, it must also expose a
    ///         <c>public</c> no-arg constructor so that the framework can instantiate the object
    ///     </para>
    /// </remarks>
    /// <seealso cref="IParser{T}" />
    public interface IEncoderDecoder<T> {
        /// <summary>
        ///     Returns the provided object as an encoded string.
        /// </summary>
        /// <para>
        ///     Even if the class is self-encodeable, this method
        ///     is always called on a new instance of the object created via the
        ///     no-arg constructor.  That is, the object shouldn't encode itself,
        ///     it should encode the object provided to it.
        /// </para>
        /// <seealso cref="FromEncodedString" />
        string ToEncodedString(T toEncode);

        /// <summary>
        ///     Converts an encoded string to an instance of the object.
        /// </summary>
        /// <para>
        ///     Here the implementing class is acting as a factory for itself
        /// </para>
        /// <seealso cref="ToEncodedString" />
        T FromEncodedString(string encodedString);
    }
}