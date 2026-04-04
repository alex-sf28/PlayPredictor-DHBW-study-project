'use client';

import { CalendarAnalysis, getApiAnalysisCalendar } from "@/client";
import { ErrorMessage } from "@/lib/message";
import { useEffect, useState } from "react";
import Analytic from "../Analytic";
import AnalyticTable from "../AnalyticTable";
import AnalyticBarChart from "../AnalyticBarChart";
import AnalyticSkeleton from "@/components/Skeletons/AnalyticSkeleton";
import AnalyticMultiBarChart from "../AnalyticMultiBarChart";

export default function CalendarAnalytics() {

    const [loading, setLoading] = useState(true);

    const [data, setData] = useState<CalendarAnalysis>({
        averageEventLength: 0,
        averageEventsPerDay: 0,
        daysWithoutEvents: 0,
        eventActivityByHourDistribution: {},
        eventActivityByWeekdayDistribution: {}
    });

    useEffect(() => {
        (async () => {
            setLoading(true);
            const res = await getApiAnalysisCalendar();
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
        <Analytic heading="Calendar Analytics" description="Insights and statistics about your calendar events">
            <AnalyticTable
                name="Event Metrics"
                description="Metrics related to your calendar events, providing insights into your scheduling patterns and habits."
                items={[
                    { field: "Average Events per Day", value: data.averageEventsPerDay ?? 0 },
                    { field: "Average Event Length (hr)", value: (data.averageEventLength ?? 0) / 60 },
                    { field: "Days Without events", value: data.daysWithoutEvents ?? 0 }
                ]} />
            <AnalyticBarChart
                dataKey="Percentage"
                name="Events per Day"
                description="The distribution of count of events held per day"
                chartData={data.eventActivityByHourDistribution ?? {}} />
            <AnalyticBarChart
                dataKey="Percentage"
                name="Activity by Hour"
                description="Visualization of your activity across different hours of the day, showing when you have the most events."
                chartData={data.eventActivityByWeekdayDistribution ?? {}} />
            <AnalyticMultiBarChart
                name="Performance by event count"
                description="The performance in relation to the Events at a given day"
                chartData={data.performanceByEventCount ?? {}}
                preSelected={["kdRatio", "krRatio"]}
            />
            <AnalyticMultiBarChart
                name="Performance by event duration"
                description="The performance in relation to the total time that events take place in a day"
                chartData={data.performanceByEventDuration ?? {}}
                preSelected={["kdRatio", "krRatio"]}
            />
            <AnalyticMultiBarChart
                name="Performance time since last event"
                description="The performance in relation to the total time between the last event"
                chartData={data.performanceByTimeSinceLastEvent ?? {}}
                preSelected={["kdRatio", "krRatio"]}
            />
        </Analytic>
    )
}