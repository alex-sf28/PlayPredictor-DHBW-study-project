"use client"

import { ActiveCalendar, UserCalendarResponse, getApiOAuthGoogleCalendarCalendars, getApiOAuthGoogleCalendarCalendarsActive, putApiOAuthGoogleCalendarCalendarsActive } from "@/client"
import { useReload } from "@/context/reload"
import { ErrorMessage, SuccessMessage } from "@/lib/message"
import { Badge, Box, Button, Checkbox, CheckboxGroup, Fieldset, For, Show, Stack } from "@chakra-ui/react"
import { useEffect, useState } from "react"
import { useController, useForm } from "react-hook-form"
import CheckBoxSkeleton from "../Skeletons/CheckBoxSkeleton"

export default function ChangeActiveCalendars() {
    const [loading, setIsLoading] = useState<boolean>(true)
    const [calendars, setCalendars] = useState<UserCalendarResponse[]>()
    const [activeCalendars, setActiveCalendars] = useState<ActiveCalendar[]>([])
    const [resLoading, setResLoading] = useState<boolean>(false);

    const { triggerReload } = useReload()

    const {
        handleSubmit,
        control,
        setValue,
        formState: { errors },
    } = useForm<{ calendars: string[] }>({
        defaultValues: {
            calendars: [],
        },
    })

    async function onSubmit(data: { calendars: string[] }) {
        setResLoading(true)
        const activeCalendars = data.calendars.map(calendarId => ({ calendarId }))
        const result = await putApiOAuthGoogleCalendarCalendarsActive({ body: activeCalendars })

        if (result.error) {
            ErrorMessage(result.error)

        } else {
            SuccessMessage("Success", "Calendars updated successfully")
            triggerReload()
        }
        setResLoading(false);
        window.location.reload()
    }

    const calendar = useController({
        control,
        name: "calendars",
        rules: {
            validate: (calendarId) => calendarId.length > 0 || "Select at least one calendar",
        }
    })

    useEffect(() => {
        (async () => {
            const cs = await getApiOAuthGoogleCalendarCalendars()
            if (!cs.error) {
                setCalendars(cs.data)
                const acs = await getApiOAuthGoogleCalendarCalendarsActive()

                if (!acs.error) {
                    setActiveCalendars(acs.data)
                    // populate form control so CheckboxGroup (controlled) shows selected items
                    setValue('calendars', acs.data.map(a => a.calendarId).filter(Boolean) as string[])

                } else {
                    ErrorMessage(acs.error)
                }
            } else {
                //ErrorMessage(cs.error)
            }

            setIsLoading(false)
        })()
    }, [])

    return <Box borderWidth="1px" padding={4}>
        <form onSubmit={handleSubmit(onSubmit)}>

            <Show when={!loading} fallback={<CheckBoxSkeleton />}>
                <Fieldset.Root invalid={!!errors.calendars}>

                    <CheckboxGroup
                        value={calendar.field.value}
                        onValueChange={calendar.field.onChange}
                        name={calendar.field.name}
                    >

                        <For each={calendars}>{(c) =>
                            <Checkbox.Root key={c.id} value={c.id ? c.id : ""} defaultChecked={activeCalendars.some(ac => ac.calendarId == c.id)} borderWidth={1} padding={2}>
                                <Checkbox.HiddenInput />
                                <Checkbox.Control />
                                <Stack>
                                    <Checkbox.Label>{c.id}
                                        <Badge marginX={2} colorPalette={c.origin == "API" ? "blue" : "green"} >{c.origin}</Badge>
                                    </Checkbox.Label>
                                    <Box textStyle="sm" color="fg.muted">
                                        {c.description ? c.description : "No description available"}
                                    </Box>
                                </Stack>
                            </Checkbox.Root>
                        }</For>
                    </CheckboxGroup>

                    {errors.calendars && (
                        <Fieldset.ErrorText>
                            {errors.calendars.message}
                        </Fieldset.ErrorText>
                    )}

                    <Button size="sm" type="submit" alignSelf="flex-start" loading={resLoading}>
                        Submit
                    </Button>
                </Fieldset.Root>

            </Show>

        </form>
    </Box>
}