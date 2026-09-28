import PageShell
    from "../components/PageShell";

import {
    reviews
} from "../data/mockData";

import "./Themes/Plan.css";

const Reviews = () => {

    const completed =
        reviews.filter(
            review =>
                review.completed
        ).length;

    const approved =
        reviews.filter(
            review =>
                review.approved
        ).length;

    return (
        <PageShell
            title="Teacher reviews"
            description={
                "Track which lecturers have finished reviewing the current schedule and whether they approved it."
            }
        >

            <section
                className={
                    "review-summary"
                }
            >

                <div>

                    <strong>
                        {completed}
                        /
                        {reviews.length}
                    </strong>

                    <span>
                        completed
                    </span>

                </div>

                <div>

                    <strong>
                        {approved}
                        /
                        {reviews.length}
                    </strong>

                    <span>
                        approved
                    </span>

                </div>

                <div>

                    <strong>
                        {
                            reviews.filter(
                                review =>
                                    review.remarks
                            ).length
                        }
                    </strong>

                    <span>
                        with remarks
                    </span>

                </div>

            </section>

            <section
                className="panel-card"
            >

                <div
                    className={
                        "table-scroll"
                    }
                >

                    <table
                        className={
                            "data-table"
                        }
                    >

                        <thead>

                        <tr>
                            <th>
                                Lecturer
                            </th>

                            <th>
                                Completed
                            </th>

                            <th>
                                Approved
                            </th>

                            <th>
                                Remarks
                            </th>
                        </tr>

                        </thead>

                        <tbody>

                        {
                            reviews.map(
                                review => (

                                    <tr
                                        key={
                                            review.lecturer
                                        }
                                    >

                                        <td>
                                            {
                                                review.lecturer
                                            }
                                        </td>

                                        <td>
                                            {
                                                review.completed
                                                    ? "Yes"
                                                    : "No"
                                            }
                                        </td>

                                        <td>
                                            {
                                                review.approved
                                                    ? "Yes"
                                                    : "No"
                                            }
                                        </td>

                                        <td>
                                            {
                                                review.remarks ||
                                                "—"
                                            }
                                        </td>

                                    </tr>

                                )
                            )
                        }

                        </tbody>

                    </table>

                </div>

            </section>

        </PageShell>
    );
};

export default Reviews;