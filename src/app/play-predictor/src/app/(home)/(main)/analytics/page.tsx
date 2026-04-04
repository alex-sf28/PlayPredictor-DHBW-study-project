import CalendarAnalytics from "@/components/Analytics/components/CalendarAnalytics";
import MatchAnalytics from "@/components/Analytics/components/MatchAnalytics";
import PerformanceAnalytics from "@/components/Analytics/components/PerformanceAnalytics";
import SessionAnalytics from "@/components/Analytics/components/SessionAnalytics";
import { Box, Heading, Tabs } from "@chakra-ui/react";

export default function AnalyticsPage() {
    return (
        <Box>
            <Heading size="4xl">Analytics</Heading>
            <p>Here you can find insights and statistics about your matches and predictions.</p>
            <Box marginTop={8}>
                <Tabs.Root defaultValue="match" variant="subtle" lazyMount width="80%">
                    <Tabs.List color="brand.solid">
                        <Tabs.Trigger value="match">
                            Match
                        </Tabs.Trigger>
                        <Tabs.Trigger value="session">
                            Session
                        </Tabs.Trigger>
                        <Tabs.Trigger value="performance">
                            Performance
                        </Tabs.Trigger>
                        <Tabs.Trigger value="calendar">
                            Calendar
                        </Tabs.Trigger>
                    </Tabs.List>
                    <Tabs.Content value="match">
                        <MatchAnalytics />
                    </Tabs.Content>
                    <Tabs.Content value="session">
                        <SessionAnalytics />
                    </Tabs.Content>
                    <Tabs.Content value="performance">
                        <PerformanceAnalytics />
                    </Tabs.Content>
                    <Tabs.Content value="calendar">
                        <CalendarAnalytics />
                    </Tabs.Content>
                </Tabs.Root>
            </Box>
        </Box>
    )
}