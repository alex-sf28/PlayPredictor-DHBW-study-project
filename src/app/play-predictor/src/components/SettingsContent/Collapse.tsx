"use client"

import { Collapsible } from "@chakra-ui/react"
import { LuChevronRight } from "react-icons/lu";

export default function Collapse({ children, title }: { children: React.ReactNode, title?: string }) {
    return (
        <Collapsible.Root>
            <Collapsible.Trigger
                paddingY="3"
                display="flex"
                gap="2"
                alignItems="center"
                cursor="pointer"
            >
                <Collapsible.Indicator
                    transition="transform 0.2s"
                    _open={{ transform: "rotate(90deg)" }}
                >
                    <LuChevronRight />
                </Collapsible.Indicator>
                {title ? title : "Details"}
            </Collapsible.Trigger>
            <Collapsible.Content paddingLeft={4}>
                {children}
            </Collapsible.Content>
        </Collapsible.Root>
    );
}