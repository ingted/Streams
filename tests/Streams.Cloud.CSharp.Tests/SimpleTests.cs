﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FsCheck.Fluent;
using NUnit.Framework;
using Nessos.Streams.CSharp;
using Nessos.Streams.Cloud.CSharp;
using Nessos.Streams.Cloud.CSharp.MBrace;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Nessos.Streams.Cloud.CSharp.Tests
{
    [Category("CloudStreams.CSharp.RunLocal")]
    public class RunLocal : CloudStreamsTests
    {
        internal override T Eval<T>(Nessos.MBrace.Cloud<T> c)
        {
            return MBrace.MBrace.RunLocal(c);
        }
    }

    [Category("CloudStreams.CSharp.Cluster")]
    public class Cluster : CloudStreamsTests
    {
        Runtime rt;

        public Cluster() {  }

        string GetFileDir([CallerFilePath]string file = "") { return file; }

        [TestFixtureSetUp]
        public void SetUp()
        {
            var version = typeof(Nessos.MBrace.Cloud).Assembly.GetName().Version.ToString(3);
            var path = Path.GetDirectoryName(this.GetFileDir());
            Settings.MBracedExecutablePath = Path.Combine(path, "../../packages/MBrace.Runtime." + version + "-alpha/tools/mbraced.exe");

            rt = Runtime.InitLocal(3);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            rt.Kill();
        }

        internal override T Eval<T>(Nessos.MBrace.Cloud<T> c)
        {
            return rt.Run(c);
        }
    }

    [TestFixture]
    abstract public class CloudStreamsTests
    {
        abstract internal T Eval<T>(Nessos.MBrace.Cloud<T> c);

        [Test]
        public void OfArray()
        {
            Spec.ForAny<int[]>(xs =>
            {
                var x = xs.AsCloudStream().Select(i => i + 1).ToArray();
                var y = xs.Select(i => i + 1).ToArray();
                return this.Eval(x).SequenceEqual(y);
            }).QuickCheckThrowOnFailure();
        }


        [Test]
        public void Select()
        {
            Spec.ForAny<int[]>(xs =>
            {
                var x = xs.AsCloudStream().Select(i => i + 1).ToArray();
                var y = xs.AsParallel().Select(i => i + 1).ToArray();
                return this.Eval(x).SequenceEqual(y);
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void Where()
        {
            Spec.ForAny<int[]>(xs =>
            {
                var x = xs.AsCloudStream().Where(i => i % 2 == 0).ToArray();
                var y = xs.AsParallel().Where(i => i % 2 == 0).ToArray();
                return this.Eval(x).SequenceEqual(y);
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void SelectMany()
        {
            Spec.ForAny<int[]>(xs =>
            {
                var x = xs.AsCloudStream().SelectMany(i => xs.AsStream()).ToArray();
                var y = xs.AsParallel().SelectMany(i => xs).ToArray();
                return this.Eval(x).SequenceEqual(y);
            }).QuickCheckThrowOnFailure();
        }


        [Test]
        public void Aggregate()
        {
            Spec.ForAny<int[]>(xs =>
            {
                var x = xs.AsCloudStream().Select(i => i + 1).Aggregate(() => 0, (acc, i) => acc + i, (left, right) => left + right);
                var y = xs.AsParallel().Select(i => i + 1).Aggregate(() => 0, (acc, i) => acc + i, (left, right) => left + right, i => i);
                return this.Eval(x) == y;
            }).QuickCheckThrowOnFailure();
        }


        [Test]
        public void Sum()
        {
            Spec.ForAny<int[]>(xs =>
            {
                var x = xs.AsCloudStream().Select(i => i + 1).Sum();
                var y = xs.AsParallel().Select(i => i + 1).Sum();
                return this.Eval(x) == y;
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void Count()
        {
            Spec.ForAny<int[]>(xs =>
            {
                var x = xs.AsCloudStream().Select(i => i + 1).Count();
                var y = xs.AsParallel().Select(i => i + 1).Count();
                return this.Eval(x) == y;
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void OrderBy()
        {
            Spec.ForAny<int[]>(xs =>
            {
                var x = xs.AsCloudStream().Select(i => i + 1).OrderBy(i => i,10).ToArray();
                var y = xs.AsParallel().Select(i => i + 1).OrderBy(i => i).Take(10).ToArray();
                return this.Eval(x).SequenceEqual(y);
            }).QuickCheckThrowOnFailure();
        }
    }
}
