'use client'

import Register from "@/components/Auth/Register";
import ConnectGoogleCalendar from "@/components/GoogleServices/ConnectGoogleCalendar";
import RouterLink from "@/components/RouterLink/RouterLink";
import SetupSkeleton from "@/components/Skeletons/SetupSkeleton";
import { useAuth } from "@/lib/authStore";
import { Box, Flex, Show, Stack, Steps, Text } from "@chakra-ui/react";
import { useEffect, useState } from "react";
import { getApiOAuthGoogleCalendarState, getApiOAuthGoogleCalendarCalendarsActive, getApiOAuthFaceitPlayers } from "@/client";
import ChangeActiveCalendars from "@/components/GoogleServices/ChangeActiveCalendars";
import { useReload } from "@/context/reload";
import ConnectFaceit from "@/components/Faceit/AddFaceitAccount";
import AddICalendarFiles from "@/components/GoogleServices/AddICalendarFiles";

const steps = [
    {
        title: "Register",
        description: <Flex flexDirection="row">
            <Stack borderWidth={1} p={4} gapY={6}>
                <Register successUri="/setup" />
                <Box>Already have an Account? Login <RouterLink path="/auth/login" colorPalette="brand">Here.</RouterLink></Box>
            </Stack>
        </Flex>,
    },
    {
        title: "Google Calender",
        description: <Stack>
            <p>Connect with your Google Calender</p>
            <ConnectGoogleCalendar />
            <p>or upload ICalender File (.ics)</p>
            <AddICalendarFiles />
        </Stack>,
    },
    {
        title: "Choose Calendars",
        description: <Stack><Text margin="auto">Choose the Calendars you want to use for predictions.</Text><ChangeActiveCalendars /></Stack>,
    },
    {
        title: "Faceit",
        description: <Stack><p>Connect with your Faceit Account</p><ConnectFaceit /></Stack>,
    },
]

export default function Setup() {

    const [loading, setIsLoading] = useState<boolean>(true)
    const [step, setStep] = useState<number>(0)

    const { user } = useAuth()
    const { reloadKey } = useReload()


    useEffect(() => {
        (async () => {
            setIsLoading(true)
            console.log("reloading..." + reloadKey)
            let steps = 0

            if (user != undefined) {
                steps += 1;
                const res = await getApiOAuthGoogleCalendarState()
                const cls = await getApiOAuthGoogleCalendarCalendarsActive()

                if (!res.error || (!cls.error && cls.data.length > 0)) {
                    steps += 1;

                    const cs = await getApiOAuthGoogleCalendarCalendarsActive()

                    if (!cs.error && cs.data.length > 0) {
                        steps += 1;

                        const player = await getApiOAuthFaceitPlayers()
                        if (!player.error) {
                            steps += 1;
                        }
                    }
                }
            }

            setStep(steps)
            setIsLoading(false)
        })()


    }, [user, reloadKey])


    return <Stack minWidth="70%">
        <Show when={!loading} fallback={<SetupSkeleton stepCount={steps.length} />}>
            <Steps.Root
                step={step}
                count={steps.length}
                colorPalette="brand">
                <Steps.List>
                    {steps.map((step, index) => (
                        <Steps.Item key={index} index={index} title={step.title}>
                            <Steps.Indicator />
                            <Steps.Title>{step.title}</Steps.Title>
                            <Steps.Separator />
                        </Steps.Item>
                    ))}
                </Steps.List>

                {steps.map((step, index) => (
                    <Steps.Content key={index} index={index}>
                        <Flex width="80%" margin="auto" p={4} justifyContent="center">
                            {step.description}
                        </Flex>
                    </Steps.Content>
                ))}
                <Steps.CompletedContent margin="auto">All steps are complete!</Steps.CompletedContent>
            </Steps.Root>
        </Show>
    </Stack>
}

