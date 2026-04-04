'use client'

import { AuthProvider, getApiAuthAuthProvider } from "@/client";
import ChangePassword from "@/components/ChangePassword/ChangePassword";
import Collapse from "@/components/SettingsContent/Collapse";
import Section from "@/components/SettingsContent/Section";
import SettingsSkeleton from "@/components/Skeletons/SettingsSkeleton";
import { ErrorMessage } from "@/lib/message";
import { Show } from "@chakra-ui/react";
import { useEffect, useState } from "react";



export default function Authentication() {

    const [authProvider, setAuthProvider] = useState<AuthProvider | null>(null)

    useEffect(() => {

        (async () => {
            const res = await getApiAuthAuthProvider()

            if (res.data?.authProvider) {
                setAuthProvider(res.data.authProvider)
            } else if (res.error) {
                ErrorMessage(res.error)
            }
        })()
    }, [])

    return (
        <>
            <Show when={authProvider != null} fallback={<SettingsSkeleton />}>
                <Section heading="Sign in Method">
                    {authProvider}
                </Section>
                <Show when={authProvider == "Password"}>
                    <Section heading="Options">
                        <Collapse title="Change Password">
                            <ChangePassword />
                        </Collapse>
                    </Section>
                </Show>
            </Show>
        </>
    );
}