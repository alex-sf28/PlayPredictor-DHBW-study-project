import { Box, Heading, Table } from "@chakra-ui/react";

interface DataTableProps {
    name: string;
    description: string;
    items: { field: string; value: number }[];
}

export default function AnalyticTable({ name, description, items }: DataTableProps) {
    return (
        <Box>
            <Heading size="md" mb={2}>{name}</Heading>
            <Heading size="sm" mb={4} color="gray.500">{description}</Heading>
            <Table.Root size="sm" striped>
                {/* <Table.Header>
                    <Table.Row>
                        <Table.ColumnHeader>Analytic</Table.ColumnHeader>
                        <Table.ColumnHeader textAlign="end">Value</Table.ColumnHeader>
                    </Table.Row>
                </Table.Header> */}
                <Table.Body>
                    {items.map((item) => (
                        <Table.Row key={item.field}>
                            <Table.Cell>{item.field}</Table.Cell>
                            <Table.Cell textAlign="end">{Math.round(item.value * 100) / 100}</Table.Cell>
                        </Table.Row>
                    ))}
                </Table.Body>
            </Table.Root>
        </Box>
    )

}