// Copyright � KaKush LLC
// Written By Steven Zawaski
// Licensed to you under the MIT license

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Zerra.Serialization;

namespace Zerra.Test
{
    [TestClass]
    public class JsonSerializerTest
    {
        [TestMethod]
        public void MatchesStandards()
        {
            var baseModel = Factory.GetAllTypesModel();
            var json1 = JsonSerializer.Serialize(baseModel);
            var json2 = Newtonsoft.Json.JsonConvert.SerializeObject(baseModel, new Newtonsoft.Json.Converters.StringEnumConverter());
            //swap serializers
            var model1 = JsonSerializer.Deserialize<AllTypesModel>(json2);
            var model2 = Newtonsoft.Json.JsonConvert.DeserializeObject<AllTypesModel>(json1);
            Factory.AssertAreEqual(model1, model2);
        }

        [TestMethod]
        public void Types()
        {
            var baseModel = Factory.GetAllTypesModel();
            var json = JsonSerializer.Serialize(baseModel);
            var model = JsonSerializer.Deserialize<AllTypesModel>(json);
            Factory.AssertAreEqual(baseModel, model);
        }

        [TestMethod]
        public void ConvertTypes()
        {
            var baseModel = Factory.GetAllTypesModel();
            var json1 = JsonSerializer.Serialize(baseModel);
            var model1 = JsonSerializer.Deserialize<AllTypesAsStringsModel>(json1);
            Factory.AssertAreEqual(baseModel, model1);

            var json2 = JsonSerializer.Serialize(model1);
            var model2 = JsonSerializer.Deserialize<AllTypesModel>(json2);
            Factory.AssertAreEqual(baseModel, model2);
        }

        [TestMethod]
        public void Numbers()
        {
            for (var i = -10; i < 10; i++)
                TestNumber(i);
            for (decimal i = -2; i < 2; i += 0.1m)
                TestNumber(i);

            TestNumber(Byte.MinValue);
            TestNumber(Byte.MaxValue);
            TestNumber(SByte.MinValue);
            TestNumber(SByte.MaxValue);

            TestNumber(Int16.MinValue);
            TestNumber(Int16.MaxValue);
            TestNumber(UInt16.MinValue);
            TestNumber(UInt16.MaxValue);

            TestNumber(Int32.MinValue);
            TestNumber(Int32.MaxValue);
            TestNumber(UInt32.MinValue);
            TestNumber(UInt32.MaxValue);

            TestNumber(Int64.MinValue);
            TestNumber(Int64.MaxValue);
            TestNumber(UInt64.MinValue);
            TestNumber(UInt64.MaxValue);

            TestNumber(Single.MinValue);
            TestNumber(Single.MaxValue);

            TestNumber(Double.MinValue);
            TestNumber(Double.MaxValue);

            TestNumber(Decimal.MinValue);
            TestNumber(Decimal.MaxValue);

            TestNumberAsString(Double.MinValue);
            TestNumberAsString(Double.MaxValue);

            TestNumberAsString(Decimal.MinValue);
            TestNumberAsString(Decimal.MaxValue);
        }
        private static void TestNumber(byte value)
        {
            var json = JsonSerializer.Serialize(value);
            var result = JsonSerializer.Deserialize<byte>(json);
            Assert.AreEqual(value, result);
        }
        private static void TestNumber(sbyte value)
        {
            var json = JsonSerializer.Serialize(value);
            var result = JsonSerializer.Deserialize<sbyte>(json);
            Assert.AreEqual(value, result);
        }
        private static void TestNumber(short value)
        {
            var json = JsonSerializer.Serialize(value);
            var result = JsonSerializer.Deserialize<short>(json);
            Assert.AreEqual(value, result);
        }
        private static void TestNumber(ushort value)
        {
            var json = JsonSerializer.Serialize(value);
            var result = JsonSerializer.Deserialize<ushort>(json);
            Assert.AreEqual(value, result);
        }
        private static void TestNumber(int value)
        {
            var json = JsonSerializer.Serialize(value);
            var result = JsonSerializer.Deserialize<int>(json);
            Assert.AreEqual(value, result);
        }
        private static void TestNumber(uint value)
        {
            var json = JsonSerializer.Serialize(value);
            var result = JsonSerializer.Deserialize<uint>(json);
            Assert.AreEqual(value, result);
        }
        private static void TestNumber(long value)
        {
            var json = JsonSerializer.Serialize(value);
            var result = JsonSerializer.Deserialize<long>(json);
            Assert.AreEqual(value, result);
        }
        private static void TestNumber(ulong value)
        {
            var json = JsonSerializer.Serialize(value);
            var result = JsonSerializer.Deserialize<ulong>(json);
            Assert.AreEqual(value, result);
        }
        private static void TestNumber(decimal value)
        {
            var json = JsonSerializer.Serialize(value);
            var result = JsonSerializer.Deserialize<decimal>(json);
            Assert.AreEqual(value, result);
        }
        private static void TestNumber(float value)
        {
            var json = JsonSerializer.Serialize(value);
            var result = JsonSerializer.Deserialize<float>(json);
            Assert.AreEqual(value, result);
        }
        private static void TestNumber(double value)
        {
            var json = JsonSerializer.Serialize(value);
            var result = JsonSerializer.Deserialize<double>(json);
            Assert.AreEqual(value, result);
        }
        private static void TestNumberAsString(double value)
        {
            var json = JsonSerializer.Serialize(value);
            var result = JsonSerializer.Deserialize<string>(json);
            Assert.AreEqual(json, result);
        }
        private static void TestNumberAsString(decimal value)
        {
            var json = JsonSerializer.Serialize(value);
            var result = JsonSerializer.Deserialize<string>(json);
            Assert.AreEqual(json, result);
        }

        [TestMethod]
        public void Pretty()
        {
            var baseModel = Factory.GetAllTypesModel();
            var json = JsonSerializer.Serialize(baseModel);
            string jsonPretty;
            using (var stringReader = new StringReader(json))
            using (var stringWriter = new StringWriter())
            {
                var jsonReader = new Newtonsoft.Json.JsonTextReader(stringReader);
                var jsonWriter = new Newtonsoft.Json.JsonTextWriter(stringWriter) { Formatting = Newtonsoft.Json.Formatting.Indented, Indentation = 4 };
                jsonWriter.WriteToken(jsonReader);
                jsonPretty = stringWriter.ToString();
            }
            var model = JsonSerializer.Deserialize<AllTypesModel>(jsonPretty);
            Factory.AssertAreEqual(baseModel, model);
        }

        [TestMethod]
        public void Nameless()
        {
            var baseModel = Factory.GetAllTypesModel();
            var json = JsonSerializer.SerializeNameless(baseModel);
            var model = JsonSerializer.DeserializeNameless<AllTypesModel>(json);
            Factory.AssertAreEqual(baseModel, model);
        }

        [TestMethod]
        public void Emptys()
        {
            var json1 = JsonSerializer.Serialize<string>(null);
            Assert.AreEqual("null", json1);

            var json2 = JsonSerializer.Serialize<string>(String.Empty);
            Assert.AreEqual("\"\"", json2);

            var json3 = JsonSerializer.Serialize<object>(null);
            Assert.AreEqual("null", json3);

            var json4 = JsonSerializer.Serialize<object>(String.Empty);
            Assert.AreEqual("{}", json4);

            var model1 = JsonSerializer.Deserialize<string>("null");
            Assert.IsNull(model1);

            var model2 = JsonSerializer.Deserialize<string>("");
            Assert.AreEqual("", model2);

            var model3 = JsonSerializer.Deserialize<string>("\"\"");
            Assert.AreEqual("", model3);

            var model4 = JsonSerializer.Deserialize<string>("{}");
            Assert.AreEqual("", model4);

            var model5 = JsonSerializer.Deserialize<object>("null");
            Assert.IsNull(model5);

            var model6 = JsonSerializer.Deserialize<object>("");
            Assert.IsNull(model6);

            var model7 = JsonSerializer.Deserialize<object>("\"\"");
            Assert.IsNull(model7);

            var model8 = JsonSerializer.Deserialize<object>("{}");
            Assert.IsNotNull(model8);
        }

        [TestMethod]
        public void Escaping()
        {
            for (var i = 0; i < 255; i++)
            {
                var c = (char)i;
                var json = JsonSerializer.Serialize(c);
                var result = JsonSerializer.Deserialize<char>(json);
                Assert.AreEqual(c, result);

                switch (c)
                {
                    case '\\':
                    case '"':
                    case '/':
                    case '\b':
                    case '\t':
                    case '\n':
                    case '\f':
                    case '\r':
                        Assert.AreEqual(4, json.Length);
                        break;
                    default:
                        if (c < ' ')
                            Assert.AreEqual(8, json.Length);
                        break;
                }
            }
        }

        [TestMethod]
        public void JsonObject()
        {
            var baseModel = Factory.GetAllTypesModel();
            var json = JsonSerializer.Serialize(baseModel);
            var jsonObject = JsonSerializer.DeserializeJsonObject(json);

            var json2 = jsonObject.ToString();

            Assert.AreEqual(json, json2);

            var model1 = new AllTypesModel();

            model1.BooleanThing = (bool)jsonObject[nameof(AllTypesModel.BooleanThing)];
            model1.ByteThing = (byte)jsonObject[nameof(AllTypesModel.ByteThing)];
            model1.SByteThing = (sbyte)jsonObject[nameof(AllTypesModel.SByteThing)];
            model1.Int16Thing = (short)jsonObject[nameof(AllTypesModel.Int16Thing)];
            model1.UInt16Thing = (ushort)jsonObject[nameof(AllTypesModel.UInt16Thing)];
            model1.Int32Thing = (int)jsonObject[nameof(AllTypesModel.Int32Thing)];
            model1.UInt32Thing = (uint)jsonObject[nameof(AllTypesModel.UInt32Thing)];
            model1.Int64Thing = (long)jsonObject[nameof(AllTypesModel.Int64Thing)];
            model1.UInt64Thing = (ulong)jsonObject[nameof(AllTypesModel.UInt64Thing)];
            model1.SingleThing = (float)jsonObject[nameof(AllTypesModel.SingleThing)];
            model1.DoubleThing = (double)jsonObject[nameof(AllTypesModel.DoubleThing)];
            model1.DecimalThing = (decimal)jsonObject[nameof(AllTypesModel.DecimalThing)];
            model1.CharThing = (char)jsonObject[nameof(AllTypesModel.CharThing)];
            model1.DateTimeThing = (DateTime)jsonObject[nameof(AllTypesModel.DateTimeThing)];
            model1.DateTimeOffsetThing = (DateTimeOffset)jsonObject[nameof(AllTypesModel.DateTimeOffsetThing)];
            model1.TimeSpanThing = (TimeSpan)jsonObject[nameof(AllTypesModel.TimeSpanThing)];
            model1.GuidThing = (Guid)jsonObject[nameof(AllTypesModel.GuidThing)];

            model1.BooleanThingNullable = (bool?)jsonObject[nameof(AllTypesModel.BooleanThingNullable)];
            model1.ByteThingNullable = (byte?)jsonObject[nameof(AllTypesModel.ByteThingNullable)];
            model1.SByteThingNullable = (sbyte?)jsonObject[nameof(AllTypesModel.SByteThingNullable)];
            model1.Int16ThingNullable = (short?)jsonObject[nameof(AllTypesModel.Int16ThingNullable)];
            model1.UInt16ThingNullable = (ushort?)jsonObject[nameof(AllTypesModel.UInt16ThingNullable)];
            model1.Int32ThingNullable = (int?)jsonObject[nameof(AllTypesModel.Int32ThingNullable)];
            model1.UInt32ThingNullable = (uint?)jsonObject[nameof(AllTypesModel.UInt32ThingNullable)];
            model1.Int64ThingNullable = (long?)jsonObject[nameof(AllTypesModel.Int64ThingNullable)];
            model1.UInt64ThingNullable = (ulong?)jsonObject[nameof(AllTypesModel.UInt64ThingNullable)];
            model1.SingleThingNullable = (float?)jsonObject[nameof(AllTypesModel.SingleThingNullable)];
            model1.DoubleThingNullable = (double?)jsonObject[nameof(AllTypesModel.DoubleThingNullable)];
            model1.DecimalThingNullable = (decimal?)jsonObject[nameof(AllTypesModel.DecimalThingNullable)];
            model1.CharThingNullable = (char?)jsonObject[nameof(AllTypesModel.CharThingNullable)];
            model1.DateTimeThingNullable = (DateTime?)jsonObject[nameof(AllTypesModel.DateTimeThingNullable)];
            model1.DateTimeOffsetThingNullable = (DateTimeOffset?)jsonObject[nameof(AllTypesModel.DateTimeOffsetThingNullable)];
            model1.TimeSpanThingNullable = (TimeSpan?)jsonObject[nameof(AllTypesModel.TimeSpanThingNullable)];
            model1.GuidThingNullable = (Guid?)jsonObject[nameof(AllTypesModel.GuidThingNullable)];

            model1.BooleanThingNullableNull = (bool?)jsonObject[nameof(AllTypesModel.BooleanThingNullableNull)];
            model1.ByteThingNullableNull = (byte?)jsonObject[nameof(AllTypesModel.ByteThingNullableNull)];
            model1.SByteThingNullableNull = (sbyte?)jsonObject[nameof(AllTypesModel.SByteThingNullableNull)];
            model1.Int16ThingNullableNull = (short?)jsonObject[nameof(AllTypesModel.Int16ThingNullableNull)];
            model1.UInt16ThingNullableNull = (ushort?)jsonObject[nameof(AllTypesModel.UInt16ThingNullableNull)];
            model1.Int32ThingNullableNull = (int?)jsonObject[nameof(AllTypesModel.Int32ThingNullableNull)];
            model1.UInt32ThingNullableNull = (uint?)jsonObject[nameof(AllTypesModel.UInt32ThingNullableNull)];
            model1.Int64ThingNullableNull = (long?)jsonObject[nameof(AllTypesModel.Int64ThingNullableNull)];
            model1.UInt64ThingNullableNull = (ulong?)jsonObject[nameof(AllTypesModel.UInt64ThingNullableNull)];
            model1.SingleThingNullableNull = (float?)jsonObject[nameof(AllTypesModel.SingleThingNullableNull)];
            model1.DoubleThingNullableNull = (double?)jsonObject[nameof(AllTypesModel.DoubleThingNullableNull)];
            model1.DecimalThingNullableNull = (decimal?)jsonObject[nameof(AllTypesModel.DecimalThingNullableNull)];
            model1.CharThingNullableNull = (char?)jsonObject[nameof(AllTypesModel.CharThingNullableNull)];
            model1.DateTimeThingNullableNull = (DateTime?)jsonObject[nameof(AllTypesModel.DateTimeThingNullableNull)];
            model1.DateTimeOffsetThingNullableNull = (DateTimeOffset?)jsonObject[nameof(AllTypesModel.DateTimeOffsetThingNullableNull)];
            model1.TimeSpanThingNullableNull = (TimeSpan?)jsonObject[nameof(AllTypesModel.TimeSpanThingNullableNull)];
            model1.GuidThingNullableNull = (Guid?)jsonObject[nameof(AllTypesModel.GuidThingNullableNull)];

            model1.StringThing = (string)jsonObject[nameof(AllTypesModel.StringThing)];
            model1.StringThingNull = (string)jsonObject[nameof(AllTypesModel.StringThingNull)];
            model1.StringThingEmpty = (string)jsonObject[nameof(AllTypesModel.StringThingEmpty)];

            model1.EnumThing = (EnumModel)Enum.Parse(typeof(EnumModel), (string)jsonObject[nameof(AllTypesModel.EnumThing)]);
            model1.EnumThingNullable = (EnumModel)Enum.Parse(typeof(EnumModel), (string)jsonObject[nameof(AllTypesModel.EnumThingNullable)]);
            model1.EnumThingNullableNull = ((string)jsonObject[nameof(AllTypesModel.EnumThingNullableNull)]) == null ? (EnumModel?)null : EnumModel.Item1;

            model1.BooleanArray = (bool[])jsonObject[nameof(AllTypesModel.BooleanArray)];
            model1.ByteArray = (byte[])jsonObject[nameof(AllTypesModel.ByteArray)];
            model1.SByteArray = (sbyte[])jsonObject[nameof(AllTypesModel.SByteArray)];
            model1.Int16Array = (short[])jsonObject[nameof(AllTypesModel.Int16Array)];
            model1.UInt16Array = (ushort[])jsonObject[nameof(AllTypesModel.UInt16Array)];
            model1.Int32Array = (int[])jsonObject[nameof(AllTypesModel.Int32Array)];
            model1.UInt32Array = (uint[])jsonObject[nameof(AllTypesModel.UInt32Array)];
            model1.Int64Array = (long[])jsonObject[nameof(AllTypesModel.Int64Array)];
            model1.UInt64Array = (ulong[])jsonObject[nameof(AllTypesModel.UInt64Array)];
            model1.SingleArray = (float[])jsonObject[nameof(AllTypesModel.SingleArray)];
            model1.DoubleArray = (double[])jsonObject[nameof(AllTypesModel.DoubleArray)];
            model1.DecimalArray = (decimal[])jsonObject[nameof(AllTypesModel.DecimalArray)];
            model1.CharArray = (char[])jsonObject[nameof(AllTypesModel.CharArray)];
            model1.DateTimeArray = (DateTime[])jsonObject[nameof(AllTypesModel.DateTimeArray)];
            model1.DateTimeOffsetArray = (DateTimeOffset[])jsonObject[nameof(AllTypesModel.DateTimeOffsetArray)];
            model1.TimeSpanArray = (TimeSpan[])jsonObject[nameof(AllTypesModel.TimeSpanArray)];
            model1.GuidArray = (Guid[])jsonObject[nameof(AllTypesModel.GuidArray)];

            model1.BooleanArrayNullable = (bool?[])jsonObject[nameof(AllTypesModel.BooleanArrayNullable)];
            model1.ByteArrayNullable = (byte?[])jsonObject[nameof(AllTypesModel.ByteArrayNullable)];
            model1.SByteArrayNullable = (sbyte?[])jsonObject[nameof(AllTypesModel.SByteArrayNullable)];
            model1.Int16ArrayNullable = (short?[])jsonObject[nameof(AllTypesModel.Int16ArrayNullable)];
            model1.UInt16ArrayNullable = (ushort?[])jsonObject[nameof(AllTypesModel.UInt16ArrayNullable)];
            model1.Int32ArrayNullable = (int?[])jsonObject[nameof(AllTypesModel.Int32ArrayNullable)];
            model1.UInt32ArrayNullable = (uint?[])jsonObject[nameof(AllTypesModel.UInt32ArrayNullable)];
            model1.Int64ArrayNullable = (long?[])jsonObject[nameof(AllTypesModel.Int64ArrayNullable)];
            model1.UInt64ArrayNullable = (ulong?[])jsonObject[nameof(AllTypesModel.UInt64ArrayNullable)];
            model1.SingleArrayNullable = (float?[])jsonObject[nameof(AllTypesModel.SingleArrayNullable)];
            model1.DoubleArrayNullable = (double?[])jsonObject[nameof(AllTypesModel.DoubleArrayNullable)];
            model1.DecimalArrayNullable = (decimal?[])jsonObject[nameof(AllTypesModel.DecimalArrayNullable)];
            model1.CharArrayNullable = (char?[])jsonObject[nameof(AllTypesModel.CharArrayNullable)];
            model1.DateTimeArrayNullable = (DateTime?[])jsonObject[nameof(AllTypesModel.DateTimeArrayNullable)];
            model1.DateTimeOffsetArrayNullable = (DateTimeOffset?[])jsonObject[nameof(AllTypesModel.DateTimeOffsetArrayNullable)];
            model1.TimeSpanArrayNullable = (TimeSpan?[])jsonObject[nameof(AllTypesModel.TimeSpanArrayNullable)];
            model1.GuidArrayNullable = (Guid?[])jsonObject[nameof(AllTypesModel.GuidArrayNullable)];

            model1.StringArray = (string[])jsonObject[nameof(AllTypesModel.StringArray)];
            model1.StringEmptyArray = (string[])jsonObject[nameof(AllTypesModel.StringEmptyArray)];

            model1.EnumArray = ((string[])jsonObject[nameof(AllTypesModel.EnumArray)]).Select(x => (EnumModel)Enum.Parse(typeof(EnumModel), x)).ToArray();
            model1.EnumArrayNullable = ((string[])jsonObject[nameof(AllTypesModel.EnumArrayNullable)]).Select(x => x == null ? (EnumModel?)null : (EnumModel)Enum.Parse(typeof(EnumModel), x)).ToArray();

            model1.BooleanList = ((bool[])jsonObject[nameof(AllTypesModel.BooleanList)]).ToList();
            model1.ByteList = ((byte[])jsonObject[nameof(AllTypesModel.ByteList)]).ToList();
            model1.SByteList = ((sbyte[])jsonObject[nameof(AllTypesModel.SByteList)]).ToList();
            model1.Int16List = ((short[])jsonObject[nameof(AllTypesModel.Int16List)]).ToList();
            model1.UInt16List = ((ushort[])jsonObject[nameof(AllTypesModel.UInt16List)]).ToList();
            model1.Int32List = ((int[])jsonObject[nameof(AllTypesModel.Int32List)]).ToList();
            model1.UInt32List = ((uint[])jsonObject[nameof(AllTypesModel.UInt32List)]).ToList();
            model1.Int64List = ((long[])jsonObject[nameof(AllTypesModel.Int64List)]).ToList();
            model1.UInt64List = ((ulong[])jsonObject[nameof(AllTypesModel.UInt64List)]).ToList();
            model1.SingleList = ((float[])jsonObject[nameof(AllTypesModel.SingleList)]).ToList();
            model1.DoubleList = ((double[])jsonObject[nameof(AllTypesModel.DoubleList)]).ToList();
            model1.DecimalList = ((decimal[])jsonObject[nameof(AllTypesModel.DecimalList)]).ToList();
            model1.CharList = ((char[])jsonObject[nameof(AllTypesModel.CharList)]).ToList();
            model1.DateTimeList = ((DateTime[])jsonObject[nameof(AllTypesModel.DateTimeList)]).ToList();
            model1.DateTimeOffsetList = ((DateTimeOffset[])jsonObject[nameof(AllTypesModel.DateTimeOffsetList)]).ToList();
            model1.TimeSpanList = ((TimeSpan[])jsonObject[nameof(AllTypesModel.TimeSpanList)]).ToList();
            model1.GuidList = ((Guid[])jsonObject[nameof(AllTypesModel.GuidList)]).ToList();

            model1.BooleanListNullable = ((bool?[])jsonObject[nameof(AllTypesModel.BooleanListNullable)]).ToList();
            model1.ByteListNullable = ((byte?[])jsonObject[nameof(AllTypesModel.ByteListNullable)]).ToList();
            model1.SByteListNullable = ((sbyte?[])jsonObject[nameof(AllTypesModel.SByteListNullable)]).ToList();
            model1.Int16ListNullable = ((short?[])jsonObject[nameof(AllTypesModel.Int16ListNullable)]).ToList();
            model1.UInt16ListNullable = ((ushort?[])jsonObject[nameof(AllTypesModel.UInt16ListNullable)]).ToList();
            model1.Int32ListNullable = ((int?[])jsonObject[nameof(AllTypesModel.Int32ListNullable)]).ToList();
            model1.UInt32ListNullable = ((uint?[])jsonObject[nameof(AllTypesModel.UInt32ListNullable)]).ToList();
            model1.Int64ListNullable = ((long?[])jsonObject[nameof(AllTypesModel.Int64ListNullable)]).ToList();
            model1.UInt64ListNullable = ((ulong?[])jsonObject[nameof(AllTypesModel.UInt64ListNullable)]).ToList();
            model1.SingleListNullable = ((float?[])jsonObject[nameof(AllTypesModel.SingleListNullable)]).ToList();
            model1.DoubleListNullable = ((double?[])jsonObject[nameof(AllTypesModel.DoubleListNullable)]).ToList();
            model1.DecimalListNullable = ((decimal?[])jsonObject[nameof(AllTypesModel.DecimalListNullable)]).ToList();
            model1.CharListNullable = ((char?[])jsonObject[nameof(AllTypesModel.CharListNullable)]).ToList();
            model1.DateTimeListNullable = ((DateTime?[])jsonObject[nameof(AllTypesModel.DateTimeListNullable)]).ToList();
            model1.DateTimeOffsetListNullable = ((DateTimeOffset?[])jsonObject[nameof(AllTypesModel.DateTimeOffsetListNullable)]).ToList();
            model1.TimeSpanListNullable = ((TimeSpan?[])jsonObject[nameof(AllTypesModel.TimeSpanListNullable)]).ToList();
            model1.GuidListNullable = ((Guid?[])jsonObject[nameof(AllTypesModel.GuidListNullable)]).ToList();

            model1.StringList = ((string[])jsonObject[nameof(AllTypesModel.StringList)]).ToList();

            model1.EnumList = (((string[])jsonObject[nameof(AllTypesModel.EnumList)]).Select(x => (EnumModel)Enum.Parse(typeof(EnumModel), x)).ToList()).ToList();
            model1.EnumListNullable = (((string[])jsonObject[nameof(AllTypesModel.EnumListNullable)]).Select(x => x == null ? (EnumModel?)null : (EnumModel)Enum.Parse(typeof(EnumModel), x)).ToArray()).ToList();

            var classThingJsonObject = jsonObject[nameof(AllTypesModel.ClassThing)];
            if (!classThingJsonObject.IsNull)
            {
                model1.ClassThing = new BasicModel();
                model1.ClassThing.Value = (int)classThingJsonObject["Value"];
            }

            var classThingNullJsonObject = jsonObject[nameof(AllTypesModel.ClassThingNull)];
            if (!classThingNullJsonObject.IsNull)
            {
                model1.ClassThingNull = new BasicModel();
                model1.ClassThingNull.Value = (int)classThingNullJsonObject["Value"];
            }


            var classArrayJsonObject = jsonObject[nameof(AllTypesModel.ClassArray)];
            var classArray = new List<BasicModel>();
            foreach (var item in classArrayJsonObject)
            {
                if (!item.IsNull)
                {
                    var obj = new BasicModel();
                    obj.Value = (int)item["Value"];
                    classArray.Add(obj);
                }
                else
                {
                    classArray.Add(null);
                }
            }
            model1.ClassArray = classArray.ToArray();

            var classEnumerableJsonObject = jsonObject[nameof(AllTypesModel.ClassEnumerable)];
            var classEnumerable = new List<BasicModel>();
            foreach (var item in classEnumerableJsonObject)
            {
                if (!item.IsNull)
                {
                    var obj = new BasicModel();
                    obj.Value = (int)item["Value"];
                    classEnumerable.Add(obj);
                }
                else
                {
                    classEnumerable.Add(null);
                }
            }
            model1.ClassEnumerable = classEnumerable;

            var classListJsonObject = jsonObject[nameof(AllTypesModel.ClassList)];
            var classList = new List<BasicModel>();
            foreach (var item in classListJsonObject)
            {
                if (!item.IsNull)
                {
                    var obj = new BasicModel();
                    obj.Value = (int)item["Value"];
                    classList.Add(obj);
                }
                else
                {
                    classList.Add(null);
                }
            }
            model1.ClassList = classList;

            var dictionaryThingJsonObject = jsonObject[nameof(AllTypesModel.DictionaryThing)];
            model1.DictionaryThing = new Dictionary<int, string>();
            model1.DictionaryThing.Add(1, (string)dictionaryThingJsonObject["1"]);
            model1.DictionaryThing.Add(2, (string)dictionaryThingJsonObject["2"]);
            model1.DictionaryThing.Add(3, (string)dictionaryThingJsonObject["3"]);
            model1.DictionaryThing.Add(4, (string)dictionaryThingJsonObject["4"]);

            var stringArrayOfArrayThingJsonObject = jsonObject[nameof(AllTypesModel.StringArrayOfArrayThing)];
            var stringList = new List<string[]>();
            foreach (var item in stringArrayOfArrayThingJsonObject)
            {
                if (item.IsNull)
                {
                    stringList.Add(null);
                    continue;
                }
                var stringSubList = new List<string>();
                foreach (var sub in item)
                {
                    stringSubList.Add((string)sub);
                }
                stringList.Add(stringSubList.ToArray());
            }
            model1.StringArrayOfArrayThing = stringList.ToArray();

            Factory.AssertAreEqual(baseModel, model1);

            var model2 = jsonObject.Bind<AllTypesModel>();
            Factory.AssertAreEqual(baseModel, model2);
        }

        [TestMethod]
        public void ExceptionObject()
        {
            var model1 = new Exception("bad things happened");
            var bytes = JsonSerializer.Serialize(model1);
            var model2 = JsonSerializer.Deserialize<Exception>(bytes);
            Assert.AreEqual(model1.Message, model2.Message);
        }

        [TestMethod]
        public void Interface()
        {
            ITestInterface model1 = new TestInterfaceImplemented()
            {
                Property1 = 5,
                Property2 = 6,
                Property3 = 7
            };
            var json = JsonSerializer.Serialize(model1);
            var model2 = JsonSerializer.Deserialize<ITestInterface>(json);

            Assert.AreEqual(5, model2.Property1);
            Assert.AreEqual(6, model2.Property2);
        }

        [TestMethod]
        public void DeserializeStream()
        {
            var model1 = Factory.GetAllTypesModel();
            var bytes = JsonSerializer.SerializeBytes(model1);
            var test = JsonSerializer.Serialize(model1);
            using (var ms = new MemoryStream(bytes))
            {
                var model2 = JsonSerializer.Deserialize<AllTypesModel>(ms);
                Factory.AssertAreEqual(model1, model2);
            }
        }

        [TestMethod]
        public void SerializeStream()
        {
            var model1 = Factory.GetAllTypesModel();
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                JsonSerializer.Serialize(ms, model1);
                bytes = ms.ToArray();
            }
            var model2 = JsonSerializer.Deserialize<AllTypesModel>(bytes);
            Factory.AssertAreEqual(model1, model2);
        }
    }
}
