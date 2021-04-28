﻿using System.IO;
using Newtonsoft.Json;
using Xunit;

namespace Chino.Common.Tests
{
    public class AppleExposureV2ConfigurationTest
    {
        private readonly string PATH_JSON = "./files/apple_exposure_v2_configuration.json";

        [Fact]
        public void TestSerializeToJson()
        {
            var appleExposureV2Configuration = new ExposureConfiguration.AppleExposureV2Configuration();
            var jsonStr = JsonConvert.SerializeObject(appleExposureV2Configuration, Formatting.Indented);
            //Logger.D(jsonStr);

            using (var sr = new StreamReader(File.OpenRead(Utils.GetFullPath(PATH_JSON))))
            {
                var expected = sr.ReadToEnd();

                Assert.Equal(expected, jsonStr);
            }
        }

        [Fact]
        public void TestDeserializeFromJson()
        {
            var expected = new ExposureConfiguration.AppleExposureV2Configuration();
            var appleExposureV1Configuration = Utils.ReadObjectFromJsonPath<ExposureConfiguration.AppleExposureV2Configuration>(PATH_JSON);

            Assert.True(expected.Equals(appleExposureV1Configuration));
        }

        [Fact]
        public void TestNotEquals()
        {
            var expected = new ExposureConfiguration.AppleExposureV2Configuration();
            expected.ImmediateDurationWeight = 20;

            var appleExposureV1Configuration = Utils.ReadObjectFromJsonPath<ExposureConfiguration.AppleExposureV2Configuration>(PATH_JSON);

            Assert.False(expected.Equals(appleExposureV1Configuration));
        }
    }
}
