"use client"

import { Player } from "@/client";
import { getApiOAuthGoogleCalendarState, getApiOAuthFaceitPlayers } from "@/client/sdk.gen";
import ConnectFaceit from "@/components/Faceit/AddFaceitAccount";
import DisconnectFaceitAccount from "@/components/Faceit/DisconnectFacit";
import AddICalendarFiles from "@/components/GoogleServices/AddICalendarFiles";
import ChangeActiveCalendars from "@/components/GoogleServices/ChangeActiveCalendars";
import ConnectGoogleCalendar from "@/components/GoogleServices/ConnectGoogleCalendar";
import DisconnectGoogleAccount from "@/components/GoogleServices/DisconnectGoogleAccount";
import Collapse from "@/components/SettingsContent/Collapse";
import Section from "@/components/SettingsContent/Section";
import SettingsSkeleton from "@/components/Skeletons/SettingsSkeleton";
import { Show } from "@chakra-ui/react";
import { useEffect, useState } from "react";

export default function ConnectedServicesSettings() {
    const [hasCalendar, setHasCalendar] = useState<boolean>(false)
    const [faceitPlayer, setFaceitPlayer] = useState<Player>()
    const [isLoading, setIsLoading] = useState<boolean>(true)

    useEffect(() => {
        (async () => {
            setIsLoading(true)
            const res = await getApiOAuthGoogleCalendarState()

            if (!res.error) {
                setHasCalendar(true)
            } else {
                setHasCalendar(false)
            }

            const faceitRes = await getApiOAuthFaceitPlayers()
            if (!faceitRes.error) {
                setFaceitPlayer(faceitRes.data ?? undefined)
            }
            setIsLoading(false)
        })()
    }, [])

    if (isLoading) return <SettingsSkeleton />

    return (<>

        <Section heading="Google Calendar">
            <Collapse title={hasCalendar ? "Change Google Account" : "Connect Google Account"}>
                <ConnectGoogleCalendar />
            </Collapse>
            <Show when={hasCalendar}>
                <Collapse title="Change Active Calendars">
                    <ChangeActiveCalendars />
                </Collapse>
            </Show>
            <Collapse title="Import calendar (ICalendar)">
                <AddICalendarFiles />
            </Collapse>
        </Section>
        <Show when={hasCalendar}>
            <Section heading="Disconnect Google Account" highlight>
                <DisconnectGoogleAccount />
            </Section>
        </Show>
        <Section heading="Faceit">
            <Collapse title={faceitPlayer ? "Change Faceit Account" : "Connect Faceit Account"}>
                <ConnectFaceit player={faceitPlayer} />
            </Collapse>
        </Section>
        <Show when={faceitPlayer}>
            <Section heading="Disconnect Faceit Account" highlight>
                <DisconnectFaceitAccount />
            </Section>
        </Show>
    </>
    )
}