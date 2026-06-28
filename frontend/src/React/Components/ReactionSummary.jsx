export default function ReactionSummary({
  totalCount = 0,
  className = '',
}) {
  const wrapperClassName = ['reaction-summary', className].filter(Boolean).join(' ');

  return (
    <div
      className={wrapperClassName}
      aria-label={`Tổng số cảm xúc: ${totalCount}`}
      title={`Tổng số cảm xúc: ${totalCount}`}
    >
      <span className="reaction-summary-total">
        <span className="reaction-summary-emoji" aria-hidden="true">❤</span>
        <span className="reaction-summary-count">{totalCount || 0}</span>
      </span>
    </div>
  );
}
