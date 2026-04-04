'use client'

import DeleteAccountButton from "@/components/DeleteAccountButton/DeleteAccountButton";
import Section from "@/components/SettingsContent/Section";
import Subsection from "@/components/SettingsContent/Subsection";
import { useAuth } from "@/lib/authStore";
import { Text } from "@chakra-ui/react";

export default function Account() {

    const { user } = useAuth()

    return (<>
        <Section heading="Details">
            <Subsection heading="Username">
                {user?.userName}
            </Subsection>
            <Subsection heading="Email">
                {user?.email}
            </Subsection>
            <Subsection heading="Created at">
                {user?.createdAt}
            </Subsection>
        </Section>
        <Section heading="Delete Account" highlight>
            <Text>Once your Account is deleted, it is gone forever.</Text>
            <DeleteAccountButton />
        </Section>
    </>
    )
}