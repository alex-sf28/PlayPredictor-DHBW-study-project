import { For, Menu, Portal } from "@chakra-ui/react"
import type { OptionMenu } from "@/types/navigation"
import RouterLink from "../RouterLink/RouterLink"


export default function OptionMenu({ children, itemList }: OptionMenu) {

    return (
        <Menu.Root positioning={{ placement: "right-end" }}>
            <Menu.Trigger rounded="full" focusRing="outside">
                {children}
            </Menu.Trigger>
            <Portal>
                <Menu.Positioner>
                    <Menu.Content>
                        <For each={itemList}>
                            {(item) => <Menu.Item value={item.value} key={item.value}>
                                {(typeof item.action == "string") ? <RouterLink path={item.action}>{item.displayName}</RouterLink> : null}
                            </Menu.Item>}
                        </For>
                    </Menu.Content>
                </Menu.Positioner>
            </Portal>
        </Menu.Root>
    )
}