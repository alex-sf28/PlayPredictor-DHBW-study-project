import { Skeleton, Stack } from "@chakra-ui/react";

export default function CheckBoxSkeleton() {
    return (
        <Stack gap={2}>
            <Skeleton height="9" width={300} />
            <Skeleton height="9" width={350} />
            <Skeleton height="9" width={250} />
            <Skeleton height="12" width={50} />
        </Stack>
    );
}