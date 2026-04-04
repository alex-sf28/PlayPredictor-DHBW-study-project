'use client';

import { getApiAnalysisMatch, MatchAnalysis } from "@/client";
import { ErrorMessage } from "@/lib/message";
import { useEffect, useState } from "react";
import Analytic from "../Analytic";
import AnalyticTable from "../AnalyticTable";
import AnalyticBarChart from "../AnalyticBarChart";
import AnalyticSkeleton from "@/components/Skeletons/AnalyticSkeleton";
import AnalyticMultiBarChart from "../AnalyticMultiBarChart";

export default function MatchAnalytics() {

    const [loading, setLoading] = useState(true);

    const [data, setData] = useState<MatchAnalysis>({
        averageMatchesPerDay: 0,
        averageMatchDuration: 0,
        averageTimeBetweenMatches: 0,
        matchesPerDayDistribution: {}
    });

    useEffect(() => {
        (async () => {
            setLoading(true);
            const res = await getApiAnalysisMatch();
            if (!res.error) {
                const matchData = res.data
                setData(matchData);
                console.log(matchData)

            } else {
                ErrorMessage(res.error);
            }
            setLoading(false);
        })();
    }, [])

    if (loading)
        return <AnalyticSkeleton />

    return (
        <Analytic heading="Match Analytics" description="Insights and statistics about your matches">
            <AnalyticTable
                name="Match Metrics"
                description="Key performance indicators for your matches"
                items={[
                    { field: "Total Match Count", value: data.totalMatchCount ?? 0 },
                    { field: "Average Matches per Day", value: data.averageMatchesPerDay ?? 0 },
                    { field: "Average Match Duration (min)", value: data.averageMatchDuration ?? 0 },
                    { field: "Average Time between Matches (hr)", value: (data.averageTimeBetweenMatches ?? 0) / 60 }
                ]} />
            <AnalyticBarChart
                dataKey="Percentage"
                name="Matches per Day"
                description="The distribution of count of matches played per day"
                chartData={data.matchesPerDayDistribution ?? {}} />
            <AnalyticBarChart
                dataKey="Percentage"
                name="Activity by Hour"
                description="Visualization of your activity across different hours of the day, showing when you are most active in playing matches."
                chartData={data.activityByHourDistribution ?? {}} />
            <AnalyticBarChart
                dataKey="Percentage"
                name="Activity by Day"
                description="Visualization of your activity across different days of the week, showing when you are most active in playing matches."
                chartData={data.activityByWeekdayDistribution ?? {}} />
            <AnalyticMultiBarChart
                name="Performance By Match-Count"
                description="Visualization of the performance in relation to the count of matches player per day"
                chartData={data.performanceByMatchesPerDay ?? {}}
                preSelected={["kdRatio", "krRatio"]}
            />
        </Analytic>
    )
}