﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text;   // Used for StringBuilder class.
using System.Linq;

namespace TraceabilityTool
{
    class ConsoleReportWriter
    {
        public static int WriteMissingReqReport()
        {
            int errorCount = 0;
            StringBuilder sb = new StringBuilder();
            int count = 0;

            // Print table header.
            sb.AppendLine("Requirement ID,Reason,Found in,Line Number");

            // Find all requirements with invalid identfiers in code or tests.

            foreach (string key in ReportGenerator.invalidRequirements.Keys)
            {
                foreach (InvalidReqDictEntry entry in ReportGenerator.invalidRequirements[key])
                {
                    sb.AppendLine(key + "," + entry.reason + "," + entry.filePath + "," + entry.lineNum.ToString());
                }
            }

            // Find all requirements not covered in code.
            foreach (string key in ReportGenerator.missingCodeCoverage.Keys)
            {
                // Check if the requirement is also not covered in tests.
                if (ReportGenerator.missingTestCoverage.ContainsKey(key))
                {
                    sb.AppendLine(key + ",Not coded and not tested," + ReportGenerator.missingCodeCoverage[key]);
                    count++;
                }
                else
                {
                    sb.AppendLine(key + ",Not coded," + ReportGenerator.missingCodeCoverage[key]);
                }
            }

            // Find all the requirements not covered in tests.
            foreach (string key in ReportGenerator.missingTestCoverage.Keys)
            {
                // Ignore the requirements that we already found missing in code.
                if (!ReportGenerator.missingCodeCoverage.ContainsKey(key))
                {
                    sb.AppendLine(key + ",Not tested," + ReportGenerator.missingTestCoverage[key]);
                }
            }

            // Count all requirements errors.
            errorCount += ReportGenerator.missingCodeCoverage.Count;
            errorCount += ReportGenerator.invalidRequirements.Count;
            errorCount += ReportGenerator.missingTestCoverage.Count;

            int newRequirementCount = 0;
            // for each requirements documents that has missing code requirements 
            foreach (string filePath in ReportGenerator.missingCodeCoverage.Values.Distinct())
            {
                // If every requirement is missing a Coded requirement,
                // then we have not coded this yet, or else people have been very sloppy reviewers
                int codeMissingCount = ReportGenerator.missingCodeCoverage.Where(t => t.Value == filePath).Count();
                if (codeMissingCount == ReportGenerator.reqDocCount[filePath])
                {
                    // this number of missing code coverage is not a problem.
                    newRequirementCount += codeMissingCount;
                }
            }

            foreach (string filePath in ReportGenerator.missingTestCoverage.Values.Distinct())
            {
                // If every requirement is missing a Tests requirement,
                // then we have not written tests for this yet, or else people have been very sloppy reviewers
                int testMissingCount = ReportGenerator.missingTestCoverage.Where(t => t.Value == filePath).Count();
                if (testMissingCount == ReportGenerator.reqDocCount[filePath])
                {
                    // this number of missing test coverage is not a problem.
                    newRequirementCount += testMissingCount;
                }
            }
            errorCount -= newRequirementCount;

            sb.AppendLine("Total invalid requirements found in code and tests," + ReportGenerator.invalidRequirements.Count.ToString());
            sb.AppendLine("Total unimplemented requirements," + ReportGenerator.missingCodeCoverage.Count.ToString());
            sb.AppendLine("Total untested requirements," + ReportGenerator.missingTestCoverage.Count.ToString());
            sb.AppendLine("Total requirements missing both implementation and tests," + count.ToString());
            sb.AppendLine("New requirements excluded," + newRequirementCount.ToString());
            sb.AppendLine("Total failing (minus new requirement exclusion)," + errorCount.ToString());

            // Output data to console.
            Console.Write(sb.ToString());

            return errorCount;
        }


    }
}