'use client'

import { getApiAnalysisSession, SessionAnalysis } from "@/client";
import { ErrorMessage } from "@/lib/message";
import { useEffect, useState } from "react";
import Analytic from "../Analytic";
import AnalyticTable from "../AnalyticTable";
import AnalyticBarChart from "../AnalyticBarChart";
import AnalyticSkeleton from "@/components/Skeletons/AnalyticSkeleton";
import AnalyticMultiBarChart from "../AnalyticMultiBarChart";

export default function SessionAnalytics() {

    const [loading, setLoading] = useState(true);

    const [data, setData] = useState<SessionAnalysis>({
        matchesPerSessionDistribution: {},
        averageMatchesPerSession: 0,
        averageTimeBetweenMatchesInSession: 0,
        averageTimeBetweenSessions: 0,
        breakTime: 0
    });

    useEffect(() => {
        (async () => {
            setLoading(true);
            const res = await getApiAnalysisSession();
            if (!res.error) {
                const sessionData = res.data
                setData(sessionData);
                console.log(sessionData)

            } else {
                ErrorMessage(res.error);
            }
            setLoading(false);
        })();
    }, [])

    if (loading)
        return <AnalyticSkeleton />

    return (
        <Analytic
            heading="Session Analytics"
            description={`Insights and statistics about your gaming sessions. A session is defined by matches, that are equal or less then ${data.breakTime} min apart from each other`}  >
            <AnalyticTable
                name="Session Metrics"
                description="Indicators for your gaming sessions"
                items={[
                    { field: "Average Matches per Session", value: data.averageMatchesPerSession ?? 0 },
                    { field: "Average Time between Matches in Session (min)", value: data.averageTimeBetweenMatchesInSession ?? 0 },
                    { field: "Average Time between Sessions (hr)", value: (data.averageTimeBetweenSessions ?? 0) / 60 }
                ]}
            />
            <AnalyticBarChart
                dataKey="Percentage"
                name="Matches per Session"
                description="The distribution of count of matches played per session"
                chartData={data.matchesPerSessionDistribution ?? {}} />
            <AnalyticMultiBarChart
                name="Performance by Match in session"
                description="The performance in relation to the match index in a session"
                chartData={data.performanceByMatchIndexSession ?? {}}
                preSelected={["kdRatio", "krRatio"]}
            />
        </Analytic>
    )

}