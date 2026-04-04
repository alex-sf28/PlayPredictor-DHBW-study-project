'use client';

import { getApiAnalysisPerformance, PerformanceAnalysis } from "@/client";
import { ErrorMessage } from "@/lib/message";
import { useEffect, useState } from "react";
import Analytic from "../Analytic";
import AnalyticTable from "../AnalyticTable";
import AnalyticSkeleton from "@/components/Skeletons/AnalyticSkeleton";
import AnalyticMultiBarChart from "../AnalyticMultiBarChart";
import { prettyLabeling } from "@/lib/helpers";

export default function PerformanceAnalytics() {
    const [loading, setLoading] = useState(true);

    const [data, setData] = useState<PerformanceAnalysis>({
        averageAdr: 0,
        averageKd: 0,
        winrate: 0,
        averageDeathsPerMatch: 0,
        averageKillsPerMatch: 0,
        performanceByWeekdayDistribution: {},
        performanceByHourDistribution: {},
    });

    useEffect(() => {
        (async () => {
            setLoading(true);
            const res = await getApiAnalysisPerformance();
            if (!res.error) {
                const performanceData = res.data
                setData(performanceData);
                console.log(performanceData)

            } else {
                ErrorMessage(res.error);
            }
            setLoading(false);
        })();
    }, [])

    if (loading)
        return <AnalyticSkeleton />

    return (
        <Analytic heading="Performance Analytics" description="Insights and statistics about your gaming Performance"  >
            <AnalyticTable
                name="Performance Metrics"
                description="Key performance indicators for your gaming sessions"
                items={[
                    ...Object.entries(data.averagePerfomance ?? {}).map(value => {
                        return { field: prettyLabeling(value[0]) + " ⌀", value: value[1] }
                    }),
                    { field: "Winrate", value: data.winrate ?? 0 }
                ]
                }
            />
            <AnalyticMultiBarChart
                name="Performance by Weekday"
                description="The distribution of performance metrics by weekday"
                preSelected={["kdRatio", "krRatio"]}
                chartData={data.performanceByWeekdayDistribution ?? {}} />

            <AnalyticMultiBarChart
                name="Basic Statistics by hour of day"
                description="The distribution of basic performance statistics by the hour of the day"
                preSelected={["kdRatio", "krRatio"]}
                chartData={data.performanceByHourDistribution ?? {}}
            />
        </Analytic>
    )
}